# Phase A — Unified command router

## Entry point

```
POST /rest/v1/rpc/naad_execute_command
Authorization: Bearer <user JWT>
{
  "p_request_id": "…",
  "p_type": "TRAVEL",
  "p_payload": { "toLocationId": "…" }
}
```

## Supported types

| type | payload keys |
|------|----------------|
| TRAVEL | toLocationId |
| SPEND / BUY | sku, locationId? |
| START_NIGHT | (none) |
| COMPLETE_NIGHT | (none) |
| PHONE_REPLY | messageId, action |
| JOIN_ROOM | locationId |
| LEAVE_ROOM | (none) |
| ATTEND_EVENT | eventId |
| RELATIONSHIP_DELTA | targetId, trust?, respect?, … |
| SET_CAREER | careerId |
| ACCEPT_OPPORTUNITY | opportunityId |
| COMMIT_MEMORY | npcId, memoryType, summary, importance? |
| SET_WORLD_PERIOD | period |

## Reads (not via router)

- `naad_get_wallet`
- `naad_get_world_clock`
- `naad_list_messages`
- `naad_list_events`
- `naad_list_room_presence`
- `naad_get_relationship`
- `naad_get_npc_greeting`
- `naad_npc_dialogue_context`

## Unity

`ICommandService.ExecuteAsync` → `SupabaseCommandService` → `naad_execute_command` only.
