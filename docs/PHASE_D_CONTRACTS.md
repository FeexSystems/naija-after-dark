# Phase D — Contract hardening

## Deliverables

| Item | Location |
|------|----------|
| Command registry + envelopes | `backend/game-api/src/contracts/commands.ts` |
| RPC catalog | `backend/game-api/src/contracts/rpc-catalog.ts` |
| Contract tests (incl. invalid SKU, duplicate requestId, same-location travel) | `contracts.test.ts` |
| Unity DTOs | `game/unity/Assets/NAAD/Networking/Contracts/CommandDtos.cs` |
| Structured `requestId` logs | `SupabaseCommandService`, `PlayerInteractionController` |

## Run tests

```bash
cd backend/game-api
node --experimental-strip-types src/contracts/contracts.test.ts
```

## Rules

1. Mutations only via `naad_execute_command`.
2. Client never invents balances or locations.
3. Idempotent replay: same `requestId` → same envelope.
