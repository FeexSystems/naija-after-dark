# NAIJA AFTER DARK — Architecture

## Authoritative Rule

> The World Model is authoritative.  
> Unity renders and interacts with the World Model.  
> Gemini interprets the World Model but cannot directly mutate authoritative state.

## Trust Boundaries

| Component              | Trust Level     | Role                                      | Mutation Rights          |
|------------------------|-----------------|-------------------------------------------|--------------------------|
| PostgreSQL / Supabase  | Authoritative   | Single source of truth                    | Full (via controlled paths) |
| game-api / Edge Functions | Trusted server | Validate commands, apply mutations, emit events | Yes (after validation) |
| ai-orchestrator        | Trusted server  | Interpret context, propose intents        | None (proposals only)    |
| Gemini                 | External AI     | Reasoning / dialogue generation           | None                     |
| Unity client           | Untrusted       | Render, input, presentation               | None (sends commands only) |

## Data Flow (mutations)

```
Unity (command + requestId)
    ↓
game-api / validated server command
    ↓
PostgreSQL constraints + business rules
    ↓
domain event
    ↓
response to client
```

Gemini never sits on the mutation path.

## Key Invariants

- Money, inventory, reputation, progression are never accepted from the client.
- Service-role keys and Gemini credentials never leave the server.
- Every meaningful state change produces a domain event.
- Request IDs provide idempotency.
- Critical invariants are enforced by PostgreSQL constraints.

## Technology Choices (Gate 0)

- Game: Unity (C#)
- Backend contracts: TypeScript
- Database: PostgreSQL via Supabase
- AI: Gemini (server-side only)
- No premature microservices
- Minimal dependencies

## Directory Mapping

- `world-model/` — schemas and registries that describe authoritative state
- `supabase/migrations/` — the actual authoritative schema
- `backend/game-api/` — command validation and mutation surface
- `backend/ai-orchestrator/` — Gemini context assembly and response validation
- `game/unity/` — untrusted presentation and input client
