# NAIJA AFTER DARK (NAAD)

Nigerian nightlife life simulation. Solo-developer vertical-slice first.

## Core Architectural Rule

**The World Model is authoritative.**  
Unity renders and interacts with the World Model.  
Gemini interprets the World Model but cannot directly mutate authoritative state.

## Repository Layout

```
naija-after-dark/
├── game/
│   └── unity/                 # Unity client (untrusted)
├── backend/
│   ├── game-api/              # Validated command API (TypeScript)
│   └── ai-orchestrator/       # Gemini interpretation layer (server-side only)
├── supabase/
│   ├── migrations/            # Authoritative PostgreSQL schema
│   ├── functions/             # Edge functions
│   └── seed/                  # Seed data
├── world-model/
│   ├── schemas/               # JSON Schema + contracts
│   └── registries/            # Static registries
├── ai/
│   ├── prompts/
│   ├── schemas/
│   └── evaluations/
├── docs/
├── tools/
└── README.md
```

## Architecture Rules (non-negotiable)

1. PostgreSQL/Supabase World Model is authoritative.
2. Unity is an untrusted client.
3. Gemini is an interpretation/reasoning layer.
4. Gemini cannot directly mutate authoritative state.
5. All important mutations go through validated server commands.
6. Every meaningful mutation should produce a domain event.
7. Never expose server secrets in Unity.
8. Never trust client-provided money, inventory, reputation or progression.
9. Avoid unnecessary dependencies.
10. Prefer simple modular architecture over premature microservices.
11. Preserve existing functionality.
12. Do not rewrite unrelated systems.
13. Add tests for new business logic.
14. Keep API contracts explicit.
15. Use TypeScript for backend contracts.
16. Use C# for Unity gameplay code.
17. Use PostgreSQL constraints for critical invariants.
18. Use request IDs for idempotency.
19. Explain any architectural change before implementing it.

## Current Gate

GATE 1 — SUPABASE FOUNDATION ✅ PASSED

- Project: `unzfqrfyejkyisalzkhc` (NAAD Project)
- Migration applied + verified live
- Pass path verified:
  Auth user → player → wallet → read player → read wallet
- Test player: `55294c60-eb72-4bda-ac5d-4c96bf03268c` (Gate1 Tester, ₦100,000)

## Next

GATE 2 — WORLD MODEL schemas (JSON Schema + contracts)
