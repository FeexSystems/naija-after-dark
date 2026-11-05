-- ===========================================================================
-- 0011 — Security hardening, part 2
--
-- Follow-up to 0010. Each function body below was produced by transforming the
-- existing definition in place (not retyped), so the only differences are the
-- ones called out in the comments.
--
--   1. naad_execute_command leaked SQLERRM to the client on any exception,
--      which exposes constraint, table and column names. It now returns a
--      stable code and logs the detail server-side with RAISE WARNING.
--      A unique_violation on request_id gets its own DUPLICATE_REQUEST code
--      instead of an opaque COMMAND_EXCEPTION.
--
--   2. naad_get_world_clock derived minutesSinceMidnight from `extract(hour
--      from game_time)` (session TimeZone) while pinning the date to UTC. The
--      two disagree under any non-UTC session; both now use UTC explicitly.
--
--   3. players_update_own allowed any authenticated user to UPDATE every
--      mutable column of their own row, contradicting the comment on the
--      policy. Narrowed to column-level grants on display_name.
--
-- Still open (needs a staging database and its own migration):
--   - command_receipts is keyed on request_id alone. Lookups are now
--     player-scoped in every function touched by 0010/0011, so a foreign
--     request_id can no longer leak another player's payload, but the primary
--     key itself should become (player_id, request_id).
--   - SET_WORLD_PERIOD remains dispatchable through the router by any
--     authenticated user; closing that needs an admin/role concept.
--   - Six mutations accept p_request_id without persisting a receipt.
-- ===========================================================================

-- ---------------------------------------------------------------------------
-- 1. naad_execute_command — stop leaking SQLERRM
-- ---------------------------------------------------------------------------
create or replace function public.naad_execute_command(
    p_request_id text,
    p_type text,
    p_payload jsonb default '{}'::jsonb
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_type text := upper(trim(coalesce(p_type, '')));
    v_payload jsonb := coalesce(p_payload, '{}'::jsonb);
begin
    if auth.uid() is null then
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'UNAUTHENTICATED',
            'errorMessage', 'Not authenticated'
        );
    end if;

    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'INVALID_REQUEST',
            'errorMessage', 'request_id is required'
        );
    end if;

    if v_type = '' then
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'INVALID_REQUEST',
            'errorMessage', 'type is required'
        );
    end if;

    case v_type
        when 'TRAVEL' then
            return public.naad_travel(
                p_request_id,
                nullif(v_payload->>'toLocationId', '')::uuid
            );

        when 'SPEND' then
            return public.naad_spend(
                p_request_id,
                v_payload->>'sku',
                nullif(v_payload->>'locationId', '')::uuid
            );

        when 'BUY' then
            -- alias of spend / legacy buy
            if v_payload ? 'sku' then
                return public.naad_spend(
                    p_request_id,
                    v_payload->>'sku',
                    nullif(v_payload->>'locationId', '')::uuid
                );
            end if;
            return public.naad_buy(
                p_request_id,
                v_payload->>'itemSku',
                nullif(v_payload->>'locationId', '')::uuid
            );

        when 'START_NIGHT' then
            return public.naad_start_night(p_request_id);

        when 'COMPLETE_NIGHT' then
            return public.naad_complete_night(p_request_id);

        when 'PHONE_REPLY' then
            return public.naad_respond_message(
                p_request_id,
                nullif(v_payload->>'messageId', '')::uuid,
                v_payload->>'action'
            );

        when 'JOIN_ROOM' then
            return public.naad_join_room(
                p_request_id,
                nullif(v_payload->>'locationId', '')::uuid
            );

        when 'LEAVE_ROOM' then
            return public.naad_leave_room(p_request_id);

        when 'ATTEND_EVENT' then
            return public.naad_attend_event(
                p_request_id,
                nullif(v_payload->>'eventId', '')::uuid
            );

        when 'RELATIONSHIP_DELTA' then
            return public.naad_apply_relationship_delta(
                p_request_id,
                nullif(v_payload->>'targetId', '')::uuid,
                coalesce((v_payload->>'trust')::integer, 0),
                coalesce((v_payload->>'respect')::integer, 0),
                coalesce((v_payload->>'affection')::integer, 0),
                coalesce((v_payload->>'loyalty')::integer, 0),
                coalesce((v_payload->>'conflict')::integer, 0),
                v_payload->>'reason'
            );

        when 'SET_CAREER' then
            return public.naad_set_career(
                p_request_id,
                v_payload->>'careerId'
            );

        when 'ACCEPT_OPPORTUNITY' then
            return public.naad_accept_opportunity(
                p_request_id,
                nullif(v_payload->>'opportunityId', '')::uuid
            );

        when 'COMMIT_MEMORY' then
            return public.naad_commit_npc_memory(
                p_request_id,
                nullif(v_payload->>'npcId', '')::uuid,
                v_payload->>'memoryType',
                v_payload->>'summary',
                coalesce((v_payload->>'importance')::real, 0.5)
            );

        when 'SET_WORLD_PERIOD' then
            return public.naad_set_world_period(v_payload->>'period');

        else
            return jsonb_build_object(
                'requestId', p_request_id,
                'success', false,
                'errorCode', 'UNSUPPORTED_COMMAND',
                'errorMessage', format('Unknown command type: %s', v_type)
            );
    end case;
exception
    when unique_violation then
        -- request_id collided after the per-player receipt lookup missed it.
        -- Never surface the constraint name or any SQLERRM text to the client.
        raise warning 'naad_execute_command duplicate request_id: %', SQLERRM;
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'DUPLICATE_REQUEST',
            'errorMessage', 'Request already processed'
        );
    when others then
        raise warning 'naad_execute_command failed: %', SQLERRM;
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'COMMAND_EXCEPTION',
            'errorMessage', 'Command failed'
        );
end;
$$;

revoke all on function public.naad_execute_command(text, text, jsonb) from public;
grant execute on function public.naad_execute_command(text, text, jsonb) to authenticated;

-- ---------------------------------------------------------------------------
-- 2. naad_get_world_clock — UTC-consistent period derivation
-- ---------------------------------------------------------------------------
create or replace function public.naad_get_world_clock()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_row public.world_state%rowtype;
    v_minutes integer;
    v_period text;
    v_date text;
begin
    if auth.uid() is null then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'UNAUTHENTICATED',
            'errorMessage', 'Not authenticated'
        );
    end if;

    select * into v_row from public.world_state where id = 1;
    if not found then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'WORLD_MISSING',
            'errorMessage', 'world_state row missing'
        );
    end if;

    -- Derive minutes from the UTC wall clock. Previously `extract(hour ...)` used
    -- the session TimeZone while the date below was pinned to UTC, so the two
    -- disagreed under any non-UTC setting.
    v_minutes := (extract(hour from v_row.game_time at time zone 'UTC') * 60
                + extract(minute from v_row.game_time at time zone 'UTC'))::integer;
    v_period := public.naad_period_from_minutes(v_minutes);
    v_date := to_char(v_row.game_time at time zone 'UTC', 'YYYY-MM-DD');

    return jsonb_build_object(
        'success', true,
        'gameDate', v_date,
        'gameTime', v_row.game_time,
        'minutesSinceMidnight', v_minutes,
        'period', v_period,
        'timeScale', v_row.time_scale,
        'weather', v_row.weather,
        'trafficLevel', v_row.traffic_level,
        'nightlifeLevel', v_row.nightlife_level
    );
end;
$$;

revoke all on function public.naad_get_world_clock() from public;
grant execute on function public.naad_get_world_clock() to authenticated;

-- ---------------------------------------------------------------------------
-- 3. players — narrow the writable surface to display_name
--
-- The rows are already restricted to the caller by the players_update_own RLS
-- policy; this restricts the columns on top of that. All other player state is
-- written by SECURITY DEFINER RPCs, which run as the owner and are unaffected.
-- ---------------------------------------------------------------------------
revoke update on public.players from anon, authenticated;
grant update (display_name) on public.players to authenticated;
