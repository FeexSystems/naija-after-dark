# NAAD game-api contracts

TypeScript command contracts for the authoritative server path.

## Gate 5

- `src/contracts/commands.ts` — `GameCommand`, `CommandResult`, TRAVEL types
- Runtime validation lives in Postgres RPC `public.naad_travel(request_id, to_location_id)`
- Unity calls the RPC via PostgREST (`/rest/v1/rpc/naad_travel`) with the user JWT

No service-role in the client. No direct wallet/location UPDATEs from Unity.
