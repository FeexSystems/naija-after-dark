-- ===========================================================================
-- 0010 — Security hardening
--
-- Three defects found in the 2026-11-05 audit. Each is a trust-boundary
-- violation, not a feature change:
--
--   1. naad_spend / naad_buy trusted the client's p_location_id. A modified
--      client could buy a CLUB_TICKET or SUYA_PLATE while standing in the
--      Apartment by passing the venue id. The authoritative location is
--      public.player_locations and nothing else.
--
--   2. naad_react_to_event_attendance was SECURITY DEFINER with no auth.uid()
--      check and — unlike every sibling function — no REVOKE. PostgreSQL grants
--      EXECUTE to PUBLIC by default, so it was callable with the anon key and
--      rewrote world_state.nightlife_level / traffic_level for all players.
--      It has zero callers in the codebase, so it is revoked outright.
--
--   3. naad_ensure_relationship was missing the REVOKE/GRANT pair every other
--      client-facing function has, leaving it executable by anon.
--
-- Deliberately NOT changed here (tracked in docs/IMPLEMENTATION_PLAN_REMAINING.md):
--   - command_receipts is still keyed on request_id alone, not (player_id,
--     request_id). Widening the PK is a data migration and needs its own pass.
--   - SET_WORLD_PERIOD is still dispatchable through the router by any
--     authenticated user; closing that needs an admin/role concept first.
-- ===========================================================================

-- ---------------------------------------------------------------------------
-- 1. naad_spend — derive the location from player_locations, never the client
-- ---------------------------------------------------------------------------
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

    select * into v_receipt from public.command_receipts
     where request_id = p_request_id and player_id = v_player;
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

    -- Trust boundary: p_location_id is accepted for wire compatibility but is
    -- never used for authorisation. The server's own record wins.
    if v_price.location_type is not null then
        select l.type
          into v_loc_type
          from public.player_locations pl
          join public.locations l on l.id = pl.location_id
         where pl.player_id = v_player;

        if v_loc_type is null then
            insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
            values (p_request_id, v_player, 'SPEND', false, 'NO_LOCATION', 'Player location is unknown');
            return jsonb_build_object('requestId', p_request_id, 'success', false,
                'errorCode', 'NO_LOCATION', 'errorMessage', 'Player location is unknown');
        end if;

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
-- 2. naad_buy — same rule; the hard-coded Suya Spot uuid was itself client-trusted
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
    v_loc_type text;
begin
    if v_player_id is null then
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'UNAUTHENTICATED');
    end if;

    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_REQUEST');
    end if;

    select * into v_receipt from public.command_receipts
     where request_id = p_request_id and player_id = v_player_id;
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
        insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
        values (p_request_id, v_player_id, 'BUY', false, 'ITEM_NOT_FOUND', 'Unknown item SKU');
        return jsonb_build_object('requestId', p_request_id, 'success', false,
            'errorCode', 'ITEM_NOT_FOUND');
    end if;

    -- Suya must be bought at the Suya Spot. Read the SERVER's record of where
    -- the player is; p_location_id is only a hint.
    if p_item_sku = 'SUYA_PLATE' then
        select l.type
          into v_loc_type
          from public.player_locations pl
          join public.locations l on l.id = pl.location_id
         where pl.player_id = v_player_id;

        if v_loc_type is distinct from 'SUYA_SPOT' then
            insert into public.command_receipts (request_id, player_id, command_type, success, error_code, error_message)
            values (p_request_id, v_player_id, 'BUY', false, 'WRONG_LOCATION', 'Buy suya at the Suya Spot');
            return jsonb_build_object('requestId', p_request_id, 'success', false,
                'errorCode', 'WRONG_LOCATION', 'errorMessage', 'Buy suya at the Suya Spot');
        end if;
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
-- 3. naad_react_to_event_attendance — was PUBLIC-executable, wrote global state
--
-- No client or server code calls this. Revoking EXECUTE from every role that
-- the Data API can assume removes the write path; service_role keeps it for
-- any future server-side simulation work.
-- ---------------------------------------------------------------------------
revoke all on function public.naad_react_to_event_attendance(uuid, integer) from public;
revoke all on function public.naad_react_to_event_attendance(uuid, integer) from anon;
revoke all on function public.naad_react_to_event_attendance(uuid, integer) from authenticated;
grant execute on function public.naad_react_to_event_attendance(uuid, integer) to service_role;

-- Defensive: even if the grant above is later widened, an api-role caller is
-- rejected inside the function.
create or replace function public.naad_react_to_event_attendance(p_event_id uuid, p_attendance integer)
returns void
language plpgsql
security definer
set search_path = public
as $$
declare
    v_ev public.world_events%rowtype;
    v_traffic int;
    v_night int;
    v_role text;
begin
    -- PostgREST/Supabase expose the JWT payload as `request.jwt.claims` (JSON).
    -- Direct SQL (migrations, psql, service jobs) has no claim set -> NULL ->
    -- allowed. Only an api JWT carries a role we must police.
    begin
        v_role := nullif(current_setting('request.jwt.claims', true), '')::jsonb ->> 'role';
    exception when others then
        v_role := null;  -- unparseable/absent claims: treat as direct SQL
    end;

    if v_role is not null and v_role <> 'service_role' then
        raise exception 'naad_react_to_event_attendance is server-internal'
            using errcode = '42501';
    end if;

    select * into v_ev from public.world_events where id = p_event_id;
    if not found then return; end if;

    -- More attendance → higher nightlife + traffic pressure
    v_night := least(100, 50 + p_attendance * 5);
    v_traffic := least(100, 40 + p_attendance * 4);

    update public.world_state
    set nightlife_level = v_night,
        traffic_level = v_traffic
    where id = 1;

    insert into public.domain_events (event_type, player_id, payload)
    values (
        'WORLD_REACTIVITY',
        null,
        jsonb_build_object(
            'cause', 'EVENT_ATTENDANCE',
            'eventId', p_event_id,
            'eventName', v_ev.name,
            'attendance', p_attendance,
            'nightlifeLevel', v_night,
            'trafficLevel', v_traffic
        )
    );
end;
$$;

revoke all on function public.naad_react_to_event_attendance(uuid, integer) from public;
revoke all on function public.naad_react_to_event_attendance(uuid, integer) from anon;
revoke all on function public.naad_react_to_event_attendance(uuid, integer) from authenticated;
grant execute on function public.naad_react_to_event_attendance(uuid, integer) to service_role;

-- ---------------------------------------------------------------------------
-- 4. naad_ensure_relationship — restore the REVOKE/GRANT convention
-- ---------------------------------------------------------------------------
revoke all on function public.naad_ensure_relationship(uuid) from public;
revoke all on function public.naad_ensure_relationship(uuid) from anon;
grant execute on function public.naad_ensure_relationship(uuid) to authenticated;

-- ---------------------------------------------------------------------------
-- 5. Belt and braces — no role other than service_role may write tables
--    directly. Today this holds only because RLS has no write policies; pinning
--    it means a future permissive policy cannot silently open a table.
-- ---------------------------------------------------------------------------
do $$
declare
    r record;
begin
    for r in
        select tablename
          from pg_tables
         where schemaname = 'public'
           and tablename <> 'players'   -- players has an intentional insert/update-own policy
    loop
        execute format('revoke insert, update, delete on public.%I from anon, authenticated', r.tablename);
    end loop;
end;
$$;
