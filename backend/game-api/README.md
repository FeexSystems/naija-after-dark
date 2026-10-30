# NAAD game-api (contracts only)

This package is **not** a deployable HTTP service. It is the TypeScript **API catalog**
and contract tests for the authoritative Supabase RPCs.

## Phase D

- `src/contracts/commands.ts` — command types + `toExecuteCommandBody`
- `src/contracts/rpc-catalog.ts` — explicit RPC list (router + reads)
- `src/contracts/contracts.test.ts` — registry, payloads, adversarial cases

```bash
cd backend/game-api
node --experimental-strip-types src/contracts/contracts.test.ts
```

Mutations: `naad_execute_command` only.  
AI dialogue: Edge Function `npc-dialogue` (never writes DB directly).
