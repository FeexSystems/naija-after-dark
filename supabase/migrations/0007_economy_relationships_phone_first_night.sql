-- GATES 11–14
-- Economy polish, relationship application, phone messages, First Night summary
-- All mutations: validated server commands + domain events + request_id idempotency

-- ---------------------------------------------------------------------------
-- GATE 11 — Economy: price registry + transport spend + wallet read helper
-- ---------------------------------------------------------------------------
create table if not exists public.price_list (
    sku text primary key,
    name text not null,
    category text not null check (category in ('FOOD','DRINK','TICKET','TRANSPORT','MISC')),
    price_ngn bigint not null check (price_ngn >= 0),
    location_type text  -- optional restriction
);

insert into public.price_list (sku, name, category, price_ngn, location_type) values
    ('SUYA_PLATE', 'Suya Plate', 'FOOD', 5000, 'SUYA_SPOT'),
    ('CLUB_TICKET', 'Nightclub Ticket', 'TICKET', 20000, 'NIGHTCLUB'),
    ('SOFT_DRINK', 'Soft Drink', 'DRINK', 1500, null),
    ('TRANSPORT_SHORT', 'Short transport (okada/danfo)', 'TRANSPORT', 4000, null),
    ('BEACH_DRINK', 'Beach drink', 'DRINK', 3000, 'BEACH')
on conflict (sku) do update set price_ngn = excluded.price_ngn;

alter table public.price_list enable row level security;
create policy "price_list_read_authenticated"
    on public.price_list for select to authenticated using (true);

-- Ensure items catalog matches price_list for buyable goods
insert into public.items (id, sku, name, category, base_price_ngn) values
    ('11111111-1111-1111-1111-111111111101', 'SUYA_PLATE', 'Suya Plate', 'FOOD', 5000),
    ('11111111-1111-1111-1111-111111111102', 'CLUB_TICKET', 'Nightclub Ticket', 'TICKET', 20000),
    ('11111111-1111-1111-1111-111111111103', 'SOFT_DRINK', 'Soft Drink', 'DRINK', 1500),
    ('11111111-1111-1111-1111-111111111104', 'TRANSPORT_SHORT', 'Short transport', 'MISC', 4000),
    ('11111111-1111-1111-1111-111111111105', 'BEACH_DRINK', 'Beach drink', 'DRINK', 3000)
on conflict (sku) do update set base_price_ngn = excluded.base_price_ngn, name = excluded.name;

create or replace function public.naad_get_wallet()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_bal bigint;
    v_currency text;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    select balance, currency into v_bal, v_currency
    from public.wallets where player_id = v_player;
    if v_bal is null then
        return jsonb_build_object('success', false, 'errorCode', 'NO_WALLET');
    end if;
    return jsonb_build_object(
        'success', true,
        'playerId', v_player,
        'currency', v_currency,
        'balance', v_bal
    );
end;
$$;

revoke all on function public.naad_get_wallet() from public;
grant execute on function public.naad_get_wallet() to authenticated;

-- Generic spend by sku (extends BUY for transport / club / beach)
create or replace function public.naad_spend(
    p_request_id text,
    p_sku text,
    p_location_id uuid default null
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_price public.price_list%rowtype;
    v_loc_type text;
    v_balance bigint;
    v_receipt public.command_receipts%rowtype;
    v_payload jsonb;
begin
    if v_player is null then
        return jsonb_build_object('requestId', p_request_id, 'success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_REQUEST');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object(
            'requestId', v_receipt.request_id, 'success', v_receipt.success,
            'errorCode', v_receipt.error_code, 'errorMessage', v_receipt.error_message,
            'payload', v_receipt.result_payload
        );
    end if;

    select * into v_price from public.price_list where sku = p_sku;
    if not found then
        return jsonb_build_object('requestId', p_request_id, 'success', false, 'errorCode', 'SKU_NOT_FOUND');
    end if;

    if v_price.location_type is not null then
        if p_location_id is null then
            return jsonb_build_object('requestId', p_request_id, 'success', false, 'errorCode', 'LOCATION_REQUIRED');
        end if;
        select type into v_loc_type from public.locations where id = p_location_id;
        if v_loc_type is distinct from v_price.location_type then
            insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
            values (p_request_id, v_player, 'SPEND', false, 'WRONG_LOCATION', 'Wrong location for this purchase');
            return jsonb_build_object('requestId', p_request_id, 'success', false,
                'errorCode', 'WRONG_LOCATION', 'errorMessage', 'Wrong location for this purchase');
        end if;
    end if;

    select balance into v_balance from public.wallets where player_id = v_player for update;
    if v_balance is null then
        return jsonb_build_object('requestId', p_request_id, 'success', false, 'errorCode', 'NO_WALLET');
    end if;
    if v_balance < v_price.price_ngn then
        insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
        values (p_request_id, v_player, 'SPEND', false, 'INSUFFICIENT_FUNDS', 'Not enough NGN');
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'INSUFFICIENT_FUNDS', 'errorMessage', 'Not enough NGN');
    end if;

    update public.wallets
    set balance = balance - v_price.price_ngn, updated_at = now()
    where player_id = v_player;

    insert into public.wallet_transactions (player_id, type, amount, currency, reason, request_id)
    values (v_player, 'SPEND', v_price.price_ngn, 'NGN', p_sku, p_request_id);

    v_payload := jsonb_build_object(
        'sku', p_sku,
        'amount', v_price.price_ngn,
        'balanceAfter', v_balance - v_price.price_ngn,
        'category', v_price.category
    );

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('MONEY_SPENT', v_player, p_request_id, v_payload);

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'SPEND', true, v_payload);

    return jsonb_build_object('requestId', p_request_id, 'success', true, 'payload', v_payload);
end;
$$;

revoke all on function public.naad_spend(text, text, uuid) from public;
grant execute on function public.naad_spend(text, text, uuid) to authenticated;

-- ---------------------------------------------------------------------------
-- GATE 12 — Relationships: ensure row + apply capped deltas (server only)
-- ---------------------------------------------------------------------------
create or replace function public.naad_ensure_relationship(p_target_id uuid)
returns uuid
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_id uuid;
begin
    if v_player is null then
        raise exception 'unauthenticated';
    end if;
    select id into v_id from public.relationships
    where subject_id = v_player and target_id = p_target_id;
    if v_id is null then
        insert into public.relationships (subject_id, target_id, relationship_type)
        values (v_player, p_target_id, 'ACQUAINTANCE')
        returning id into v_id;
    end if;
    return v_id;
end;
$$;

create or replace function public.naad_apply_relationship_delta(
    p_request_id text,
    p_target_id uuid,
    p_trust integer default 0,
    p_respect integer default 0,
    p_affection integer default 0,
    p_loyalty integer default 0,
    p_conflict integer default 0,
    p_reason text default null
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_rel public.relationships%rowtype;
    v_receipt public.command_receipts%rowtype;
    v_payload jsonb;
    v_dt int := greatest(-10, least(10, coalesce(p_trust, 0)));
    v_dr int := greatest(-10, least(10, coalesce(p_respect, 0)));
    v_da int := greatest(-10, least(10, coalesce(p_affection, 0)));
    v_dl int := greatest(-10, least(10, coalesce(p_loyalty, 0)));
    v_dc int := greatest(-10, least(10, coalesce(p_conflict, 0)));
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_REQUEST');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object(
            'requestId', v_receipt.request_id, 'success', v_receipt.success,
            'payload', v_receipt.result_payload
        );
    end if;

    perform public.naad_ensure_relationship(p_target_id);

    update public.relationships set
        trust = greatest(0, least(100, trust + v_dt)),
        respect = greatest(0, least(100, respect + v_dr)),
        affection = greatest(0, least(100, affection + v_da)),
        loyalty = greatest(0, least(100, loyalty + v_dl)),
        conflict = greatest(0, least(100, conflict + v_dc)),
        updated_at = now()
    where subject_id = v_player and target_id = p_target_id
    returning * into v_rel;

    v_payload := jsonb_build_object(
        'targetId', p_target_id,
        'trust', v_rel.trust,
        'respect', v_rel.respect,
        'affection', v_rel.affection,
        'loyalty', v_rel.loyalty,
        'conflict', v_rel.conflict,
        'deltas', jsonb_build_object(
            'trust', v_dt, 'respect', v_dr, 'affection', v_da, 'loyalty', v_dl, 'conflict', v_dc
        ),
        'reason', p_reason
    );

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('RELATIONSHIP_CHANGED', v_player, p_request_id, v_payload);

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'RELATIONSHIP_DELTA', true, v_payload);

    return jsonb_build_object('requestId', p_request_id, 'success', true, 'payload', v_payload);
end;
$$;

revoke all on function public.naad_apply_relationship_delta(text, uuid, integer, integer, integer, integer, integer, text) from public;
grant execute on function public.naad_apply_relationship_delta(text, uuid, integer, integer, integer, integer, integer, text) to authenticated;

create or replace function public.naad_get_relationship(p_target_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_rel public.relationships%rowtype;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    perform public.naad_ensure_relationship(p_target_id);
    select * into v_rel from public.relationships
    where subject_id = v_player and target_id = p_target_id;
    return jsonb_build_object(
        'success', true,
        'targetId', p_target_id,
        'trust', v_rel.trust,
        'respect', v_rel.respect,
        'affection', v_rel.affection,
        'loyalty', v_rel.loyalty,
        'conflict', v_rel.conflict,
        'relationshipType', v_rel.relationship_type
    );
end;
$$;

revoke all on function public.naad_get_relationship(uuid) from public;
grant execute on function public.naad_get_relationship(uuid) to authenticated;

-- ---------------------------------------------------------------------------
-- GATE 13 — Phone: messages + reply actions
-- ---------------------------------------------------------------------------
create table if not exists public.messages (
    id uuid primary key default gen_random_uuid(),
    sender_type text not null check (sender_type in ('NPC','PLAYER','SYSTEM')),
    sender_id text not null,
    recipient_player_id uuid not null references public.players(id) on delete cascade,
    body text not null check (char_length(trim(body)) between 1 and 500),
    thread_key text not null default 'general',
    action_options jsonb not null default '[]'::jsonb,
    chosen_action text,
    read_at timestamptz,
    created_at timestamptz not null default now()
);

create index if not exists messages_recipient_idx on public.messages (recipient_player_id, created_at desc);

alter table public.messages enable row level security;
create policy "messages_read_own"
    on public.messages for select to authenticated
    using (recipient_player_id = auth.uid());

create or replace function public.naad_list_messages()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_msgs jsonb;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    select coalesce(jsonb_agg(to_jsonb(m.*) order by m.created_at desc), '[]'::jsonb)
    into v_msgs
    from (
        select * from public.messages
        where recipient_player_id = v_player
        order by created_at desc
        limit 50
    ) m;
    return jsonb_build_object('success', true, 'messages', v_msgs);
end;
$$;

revoke all on function public.naad_list_messages() from public;
grant execute on function public.naad_list_messages() to authenticated;

create or replace function public.naad_seed_tunde_beach_invite()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_id uuid;
    v_tunde text := 'e1111111-1111-1111-1111-111111111101';
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    -- One open invite per player
    if exists (
        select 1 from public.messages
        where recipient_player_id = v_player
          and thread_key = 'tunde_beach_invite'
          and chosen_action is null
    ) then
        select id into v_id from public.messages
        where recipient_player_id = v_player and thread_key = 'tunde_beach_invite' and chosen_action is null
        limit 1;
        return jsonb_build_object('success', true, 'messageId', v_id, 'deduped', true);
    end if;

    insert into public.messages (
        sender_type, sender_id, recipient_player_id, body, thread_key, action_options
    ) values (
        'NPC', v_tunde, v_player,
        'Beach dey hot tonight. You pulling up?',
        'tunde_beach_invite',
        '["GO","ASK_DETAILS","DECLINE"]'::jsonb
    ) returning id into v_id;

    insert into public.domain_events (event_type, player_id, payload)
    values ('PHONE_MESSAGE_RECEIVED', v_player,
        jsonb_build_object('messageId', v_id, 'from', 'Tunde', 'thread', 'tunde_beach_invite'));

    return jsonb_build_object('success', true, 'messageId', v_id);
end;
$$;

revoke all on function public.naad_seed_tunde_beach_invite() from public;
grant execute on function public.naad_seed_tunde_beach_invite() to authenticated;

create or replace function public.naad_respond_message(
    p_request_id text,
    p_message_id uuid,
    p_action text
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_msg public.messages%rowtype;
    v_receipt public.command_receipts%rowtype;
    v_action text := upper(trim(p_action));
    v_payload jsonb;
    v_tunde uuid := 'e1111111-1111-1111-1111-111111111101';
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object('requestId', p_request_id, 'success', v_receipt.success, 'payload', v_receipt.result_payload);
    end if;

    select * into v_msg from public.messages
    where id = p_message_id and recipient_player_id = v_player;
    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'MESSAGE_NOT_FOUND');
    end if;
    if v_msg.chosen_action is not null then
        return jsonb_build_object('success', false, 'errorCode', 'ALREADY_RESPONDED');
    end if;
    if not (v_msg.action_options ? v_action) and not (v_msg.action_options @> to_jsonb(v_action)) then
        -- action_options is json array of strings
        if not exists (
            select 1 from jsonb_array_elements_text(v_msg.action_options) a where a = v_action
        ) then
            return jsonb_build_object('success', false, 'errorCode', 'INVALID_ACTION');
        end if;
    end if;

    update public.messages
    set chosen_action = v_action, read_at = coalesce(read_at, now())
    where id = p_message_id;

    v_payload := jsonb_build_object(
        'messageId', p_message_id,
        'action', v_action,
        'threadKey', v_msg.thread_key
    );

    -- Relationship nudge for beach invite
    if v_msg.thread_key = 'tunde_beach_invite' then
        if v_action = 'GO' then
            perform public.naad_apply_relationship_delta(
                p_request_id || ':rel', v_tunde, 5, 3, 2, 1, 0, 'Accepted beach invite'
            );
        elsif v_action = 'DECLINE' then
            perform public.naad_apply_relationship_delta(
                p_request_id || ':rel', v_tunde, -1, -2, 0, 0, 2, 'Declined beach invite'
            );
        end if;
    end if;

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('PHONE_MESSAGE_RESPONDED', v_player, p_request_id, v_payload);

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'PHONE_REPLY', true, v_payload);

    return jsonb_build_object('requestId', p_request_id, 'success', true, 'payload', v_payload);
end;
$$;

revoke all on function public.naad_respond_message(text, uuid, text) from public;
grant execute on function public.naad_respond_message(text, uuid, text) to authenticated;

-- ---------------------------------------------------------------------------
-- GATE 14 — First Night session + summary
-- ---------------------------------------------------------------------------
create table if not exists public.night_sessions (
    id uuid primary key default gen_random_uuid(),
    player_id uuid not null references public.players(id) on delete cascade,
    started_at timestamptz not null default now(),
    ended_at timestamptz,
    status text not null default 'ACTIVE'
        check (status in ('ACTIVE','COMPLETED','ABANDONED')),
    summary jsonb,
    unique (player_id, started_at)
);

create index if not exists night_sessions_player_idx
    on public.night_sessions (player_id, started_at desc);

alter table public.night_sessions enable row level security;
create policy "night_sessions_read_own"
    on public.night_sessions for select to authenticated
    using (player_id = auth.uid());

create or replace function public.naad_start_night(p_request_id text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_id uuid;
    v_receipt public.command_receipts%rowtype;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object('requestId', p_request_id, 'success', v_receipt.success, 'payload', v_receipt.result_payload);
    end if;

    -- Close any stale active night
    update public.night_sessions set status = 'ABANDONED', ended_at = now()
    where player_id = v_player and status = 'ACTIVE';

    insert into public.night_sessions (player_id, status)
    values (v_player, 'ACTIVE')
    returning id into v_id;

    -- Seed phone invite
    perform public.naad_seed_tunde_beach_invite();

    -- Start evening period
    perform public.naad_set_world_period('NIGHT');

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('NIGHT_STARTED', v_player, p_request_id, jsonb_build_object('sessionId', v_id));

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'START_NIGHT', true, jsonb_build_object('sessionId', v_id));

    return jsonb_build_object('requestId', p_request_id, 'success', true,
        'payload', jsonb_build_object('sessionId', v_id));
end;
$$;

revoke all on function public.naad_start_night(text) from public;
grant execute on function public.naad_start_night(text) to authenticated;

create or replace function public.naad_complete_night(p_request_id text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_session public.night_sessions%rowtype;
    v_spent bigint;
    v_tx_count int;
    v_people int;
    v_trust int;
    v_rel jsonb;
    v_summary jsonb;
    v_receipt public.command_receipts%rowtype;
    v_tunde uuid := 'e1111111-1111-1111-1111-111111111101';
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object('requestId', p_request_id, 'success', v_receipt.success, 'payload', v_receipt.result_payload);
    end if;

    select * into v_session from public.night_sessions
    where player_id = v_player and status = 'ACTIVE'
    order by started_at desc limit 1;

    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'NO_ACTIVE_NIGHT');
    end if;

    select coalesce(sum(amount), 0), count(*) into v_spent, v_tx_count
    from public.wallet_transactions
    where player_id = v_player and type = 'SPEND' and created_at >= v_session.started_at;

    select count(distinct target_id) into v_people
    from public.relationships
    where subject_id = v_player and updated_at >= v_session.started_at;

    select trust into v_trust from public.relationships
    where subject_id = v_player and target_id = v_tunde;

    v_summary := jsonb_build_object(
        'title', 'YOUR NIGHT',
        'spentNgn', v_spent,
        'transactions', v_tx_count,
        'peopleMet', coalesce(v_people, 0),
        'newConnection', case when v_trust is not null then 'Tunde' else null end,
        'trustWithTunde', v_trust,
        'bestMoment', case
            when v_spent > 0 then 'Night out on the district'
            else 'Quiet night'
        end,
        'tomorrow', jsonb_build_object(
            'opportunities', 2,
            'invitations', 1,
            'unresolved', 1
        )
    );

    update public.night_sessions
    set status = 'COMPLETED', ended_at = now(), summary = v_summary
    where id = v_session.id;

    -- Return home
    insert into public.player_locations (player_id, location_id, updated_at)
    values (v_player, 'a1111111-1111-1111-1111-111111111101', now())
    on conflict (player_id) do update
        set location_id = excluded.location_id, updated_at = now();

    perform public.naad_set_world_period('AFTER_HOURS');

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('NIGHT_COMPLETED', v_player, p_request_id, v_summary);

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'COMPLETE_NIGHT', true, v_summary);

    return jsonb_build_object(
        'requestId', p_request_id,
        'success', true,
        'payload', v_summary
    );
end;
$$;

revoke all on function public.naad_complete_night(text) from public;
grant execute on function public.naad_complete_night(text) to authenticated;
