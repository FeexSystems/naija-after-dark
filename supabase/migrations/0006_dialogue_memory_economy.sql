-- GATE 8/9/10 — Dialogue lines, NPC memory, buy command
-- Gemini proposes; server validates; only server mutates.

-- ---------------------------------------------------------------------------
-- Deterministic dialogue lines
-- ---------------------------------------------------------------------------
create table if not exists public.npc_dialogue_lines (
    id uuid primary key default gen_random_uuid(),
    npc_id uuid not null references public.npcs(id) on delete cascade,
    context_key text not null default 'GREETING',
    line_text text not null,
    sort_order integer not null default 0,
    constraint npc_dialogue_lines_text_not_empty check (char_length(trim(line_text)) > 0)
);

create index if not exists npc_dialogue_lines_npc_ctx_idx
    on public.npc_dialogue_lines (npc_id, context_key);

alter table public.npc_dialogue_lines enable row level security;
create policy "npc_dialogue_lines_read_authenticated"
    on public.npc_dialogue_lines for select to authenticated using (true);

insert into public.npc_dialogue_lines (npc_id, context_key, line_text, sort_order) values
    ('e1111111-1111-1111-1111-111111111102', 'GREETING', 'Ah, customer! Fresh suya dey hot for fire.', 0),
    ('e1111111-1111-1111-1111-111111111102', 'GREETING', 'Welcome. You want beef or chicken?', 1),
    ('e1111111-1111-1111-1111-111111111102', 'BUY', 'Na ₦5,000 for plate. E go sweet you.', 0),
    ('e1111111-1111-1111-1111-111111111103', 'GREETING', 'The light for this beach tonight go slap.', 0),
    ('e1111111-1111-1111-1111-111111111104', 'GREETING', 'I just pushed a build. Brain don tire small.', 0),
    ('e1111111-1111-1111-1111-111111111105', 'GREETING', 'You made it. Table for us later if you pull up.', 0)
on conflict do nothing;

-- ---------------------------------------------------------------------------
-- NPC memories (validated only)
-- ---------------------------------------------------------------------------
create table if not exists public.npc_memories (
    id uuid primary key default gen_random_uuid(),
    npc_id uuid not null references public.npcs(id) on delete cascade,
    player_id uuid not null references public.players(id) on delete cascade,
    memory_type text not null,
    summary text not null,
    importance real not null default 0.5
        check (importance >= 0 and importance <= 1),
    created_at timestamptz not null default now(),
    constraint npc_memories_type_check check (
        memory_type in ('FAVOR','INSULT','SHARED_EVENT','PROMISE','TRANSACTION','INTRO','OTHER')
    ),
    constraint npc_memories_summary_len check (char_length(trim(summary)) between 1 and 280)
);

create index if not exists npc_memories_npc_player_idx
    on public.npc_memories (npc_id, player_id);
create index if not exists npc_memories_importance_idx
    on public.npc_memories (importance desc);

alter table public.npc_memories enable row level security;
create policy "npc_memories_read_own"
    on public.npc_memories for select to authenticated
    using (player_id = auth.uid());

-- ---------------------------------------------------------------------------
-- Wallet ledger (for BUY)
-- ---------------------------------------------------------------------------
create table if not exists public.wallet_transactions (
    id uuid primary key default gen_random_uuid(),
    player_id uuid not null references public.players(id) on delete cascade,
    type text not null check (type in ('EARN','SPEND','TRANSFER_IN','TRANSFER_OUT','ADJUST')),
    amount bigint not null check (amount > 0),
    currency text not null default 'NGN' check (currency = 'NGN'),
    reason text,
    related_entity_type text,
    related_entity_id uuid,
    request_id text,
    created_at timestamptz not null default now()
);

create unique index if not exists wallet_transactions_request_id_uidx
    on public.wallet_transactions (request_id)
    where request_id is not null;

alter table public.wallet_transactions enable row level security;
create policy "wallet_transactions_read_own"
    on public.wallet_transactions for select to authenticated
    using (player_id = auth.uid());

-- ---------------------------------------------------------------------------
-- Deterministic dialogue fetch
-- ---------------------------------------------------------------------------
create or replace function public.naad_get_npc_greeting(p_npc_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_npc public.npcs%rowtype;
    v_line text;
begin
    if auth.uid() is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_npc from public.npcs where id = p_npc_id;
    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'NPC_NOT_FOUND');
    end if;

    select line_text into v_line
    from public.npc_dialogue_lines
    where npc_id = p_npc_id and context_key = 'GREETING'
    order by random()
    limit 1;

    if v_line is null then
        v_line := v_npc.name || ' nods at you.';
    end if;

    return jsonb_build_object(
        'success', true,
        'npcId', v_npc.id,
        'npcName', v_npc.name,
        'dialogueMode', v_npc.dialogue_mode,
        'dialogue', v_line,
        'emotion', 'neutral'
    );
end;
$$;

revoke all on function public.naad_get_npc_greeting(uuid) from public;
grant execute on function public.naad_get_npc_greeting(uuid) to authenticated;

-- ---------------------------------------------------------------------------
-- BUY SUYA (server-validated spend)
-- ---------------------------------------------------------------------------
create or replace function public.naad_buy(
    p_request_id text,
    p_item_sku text,
    p_location_id uuid
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player_id uuid := auth.uid();
    v_item public.items%rowtype;
    v_balance bigint;
    v_receipt public.command_receipts%rowtype;
    v_payload jsonb;
begin
    if v_player_id is null then
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'UNAUTHENTICATED');
    end if;

    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_REQUEST');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object(
            'requestId', v_receipt.request_id,
            'success', v_receipt.success,
            'errorCode', v_receipt.error_code,
            'errorMessage', v_receipt.error_message,
            'payload', v_receipt.result_payload
        );
    end if;

    select * into v_item from public.items where sku = p_item_sku;
    if not found then
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'ITEM_NOT_FOUND');
    end if;

    -- Must be at location for suya (simple rule)
    if p_item_sku = 'SUYA_PLATE' and p_location_id is distinct from 'a1111111-1111-1111-1111-111111111102' then
        insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
        values (p_request_id, v_player_id, 'BUY', false, 'WRONG_LOCATION', 'Buy suya at the Suya Spot');
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'WRONG_LOCATION', 'errorMessage', 'Buy suya at the Suya Spot');
    end if;

    select balance into v_balance from public.wallets where player_id = v_player_id for update;
    if v_balance is null then
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'NO_WALLET');
    end if;

    if v_balance < v_item.base_price_ngn then
        insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
        values (p_request_id, v_player_id, 'BUY', false, 'INSUFFICIENT_FUNDS', 'Not enough NGN');
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'INSUFFICIENT_FUNDS', 'errorMessage', 'Not enough NGN');
    end if;

    update public.wallets
    set balance = balance - v_item.base_price_ngn, updated_at = now()
    where player_id = v_player_id;

    insert into public.wallet_transactions (
        player_id, type, amount, currency, reason, related_entity_type, related_entity_id, request_id
    ) values (
        v_player_id, 'SPEND', v_item.base_price_ngn, 'NGN', p_item_sku, 'ITEM', v_item.id, p_request_id
    );

    v_payload := jsonb_build_object(
        'sku', v_item.sku,
        'amount', v_item.base_price_ngn,
        'balanceAfter', v_balance - v_item.base_price_ngn
    );

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('ITEM_PURCHASED', v_player_id, p_request_id, v_payload);

    insert into public.command_receipts (
        request_id, player_id, command_type, success, result_payload
    ) values (p_request_id, v_player_id, 'BUY', true, v_payload);

    return jsonb_build_object(
        'requestId', p_request_id,
        'success', true,
        'payload', v_payload
    );
end;
$$;

revoke all on function public.naad_buy(text, text, uuid) from public;
grant execute on function public.naad_buy(text, text, uuid) to authenticated;

-- ---------------------------------------------------------------------------
-- Memory commit (server-validated; never raw AI write without checks)
-- ---------------------------------------------------------------------------
create or replace function public.naad_commit_npc_memory(
    p_request_id text,
    p_npc_id uuid,
    p_memory_type text,
    p_summary text,
    p_importance real
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player_id uuid := auth.uid();
    v_id uuid;
    v_imp real;
begin
    if v_player_id is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    if p_summary is null or char_length(trim(p_summary)) < 3 or char_length(p_summary) > 280 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_SUMMARY');
    end if;

    if p_memory_type not in ('FAVOR','INSULT','SHARED_EVENT','PROMISE','TRANSACTION','INTRO','OTHER') then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_TYPE');
    end if;

    if not exists (select 1 from public.npcs where id = p_npc_id) then
        return jsonb_build_object('success', false, 'errorCode', 'NPC_NOT_FOUND');
    end if;

    -- Do not spam: skip near-duplicate recent summary
    if exists (
        select 1 from public.npc_memories
        where npc_id = p_npc_id and player_id = v_player_id
          and summary = trim(p_summary)
          and created_at > now() - interval '1 day'
    ) then
        return jsonb_build_object('success', true, 'deduped', true);
    end if;

    v_imp := greatest(0, least(1, coalesce(p_importance, 0.5)));

    insert into public.npc_memories (npc_id, player_id, memory_type, summary, importance)
    values (p_npc_id, v_player_id, p_memory_type, trim(p_summary), v_imp)
    returning id into v_id;

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values (
        'NPC_MEMORY_CREATED',
        v_player_id,
        p_request_id,
        jsonb_build_object('memoryId', v_id, 'npcId', p_npc_id, 'type', p_memory_type)
    );

    return jsonb_build_object('success', true, 'memoryId', v_id);
end;
$$;

revoke all on function public.naad_commit_npc_memory(text, uuid, text, text, real) from public;
grant execute on function public.naad_commit_npc_memory(text, uuid, text, text, real) to authenticated;

-- Context pack for AI orchestrator (read-only aggregation)
create or replace function public.naad_npc_dialogue_context(p_npc_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player_id uuid := auth.uid();
    v_npc jsonb;
    v_rel jsonb;
    v_memories jsonb;
    v_world jsonb;
    v_location jsonb;
begin
    if v_player_id is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select to_jsonb(n.*) into v_npc from public.npcs n where n.id = p_npc_id;
    if v_npc is null then
        return jsonb_build_object('success', false, 'errorCode', 'NPC_NOT_FOUND');
    end if;

    select to_jsonb(r.*) into v_rel
    from public.relationships r
    where r.subject_id = v_player_id and r.target_id = p_npc_id
    limit 1;

    select coalesce(jsonb_agg(to_jsonb(m.*) order by m.importance desc, m.created_at desc), '[]'::jsonb)
    into v_memories
    from (
        select * from public.npc_memories
        where npc_id = p_npc_id and player_id = v_player_id
        order by importance desc, created_at desc
        limit 5
    ) m;

    select public.naad_get_world_clock() into v_world;

    select to_jsonb(l.*) into v_location
    from public.locations l
    where l.id = (v_npc->>'current_location_id')::uuid;

    return jsonb_build_object(
        'success', true,
        'playerId', v_player_id,
        'npc', v_npc,
        'relationship', v_rel,
        'memories', v_memories,
        'world', v_world,
        'location', v_location
    );
end;
$$;

revoke all on function public.naad_npc_dialogue_context(uuid) from public;
grant execute on function public.naad_npc_dialogue_context(uuid) to authenticated;
