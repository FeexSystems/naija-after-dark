-- Phase A — Unified command router
-- Unity sends one mutation entry point: naad_execute_command(request_id, type, payload)
-- Dispatches to existing validated handlers. Reads stay on dedicated RPCs.

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
    when others then
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'COMMAND_EXCEPTION',
            'errorMessage', SQLERRM
        );
end;
$$;

revoke all on function public.naad_execute_command(text, text, jsonb) from public;
grant execute on function public.naad_execute_command(text, text, jsonb) to authenticated;

comment on function public.naad_execute_command(text, text, jsonb) is
    'Phase A unified mutation entry. Reads remain on dedicated RPCs.';
