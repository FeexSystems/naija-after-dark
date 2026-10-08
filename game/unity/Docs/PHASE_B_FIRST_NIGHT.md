# Phase B — First Night (Unity)

## Goal

Playable client loop against production:

```
Auth → START_NIGHT → phone GO → TRAVEL suya → SPEND suya
  → TRAVEL club → SPEND ticket → COMPLETE_NIGHT → summary
```

All mutations: `ICommandService.ExecuteAsync` → `naad_execute_command`.

## Scene setup

1. Bootstrap scene with `NAADApplicationRoot` + `SupabaseConfig` (anon key).
2. After auth succeeds, load Main (or stay in same scene).
3. Empty GameObject → add:
   - `FirstNightController`
   - `FirstNightHud`
4. Play. Use HUD buttons or enable **Auto Run After Auth** on the controller.

## HUD actions

| Button | Command |
|--------|---------|
| Start Night | `START_NIGHT` |
| GO / ASK / DECLINE | `PHONE_REPLY` |
| Suya Spot + Buy | `TRAVEL` + `SPEND` SUYA_PLATE |
| Club + Ticket | `TRAVEL` + `SPEND` CLUB_TICKET |
| Complete Night | `COMPLETE_NIGHT` |
| Run Full Night | full sequence |

## Pass condition

One device/Editor run ends on **Summary** with spent amount and wallet balance updated from server.

## Notes

- Reads (messages, wallet) still use `IPhoneService` RPCs.
- No service-role in client.
- Stub services allow offline UI smoke tests without SupabaseConfig.
