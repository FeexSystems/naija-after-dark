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

GATES 15–20 ✅ foundation verified on live DB

- **15** Rooms + presence (ephemeral) — join/leave/pose verified
- **16** Mobile optimization checklist (measure on device)
- **17** 10 locations + 5 careers
- **18** Social opportunities (Tunde trust → Meet the DJ)
- **19** World events (Beach Party, Club Night) + attendance
- **20** Reactivity: attendance → nightlife/traffic

## Next architecture

See **`docs/NEXT_ARCHITECTURE.md`** · Phase A docs: **`docs/PHASE_A_COMMAND_ROUTER.md`**

- **Phase A ✅** — `naad_execute_command` live
- **Phase B ✅** — Unity `FirstNightController` + HUD (mutations via command router)
- **Phase C** — deploy Gemini Edge Function  
- **Phase D** — contract hardening  
- **Phase E** — Realtime presence (optional)  
- **Phase F** — Gate 16 mobile profiling  

Do not add microservices or expand the city until First Night is playable.
