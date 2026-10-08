# NAIJA AFTER DARK (NAAD)

A Nigerian nightlife life-simulation game prototype built as a solo-developer vertical slice. The project is designed around a strict authoritative world model, server-validated commands, and a simple architecture that keeps gameplay state consistent while allowing an AI interpretation layer to assist without directly mutating core game state.

## Overview

NAIJA AFTER DARK is a simulation-first game about movement, social life, money, nightlife flow, and progression inside a stylized Nigerian city environment. The current phase is intentionally focused on a playable foundation and a clear architecture rather than broad scope expansion.

The repository is organized around a few clear principles:

- The World Model is authoritative.
- Unity is a presentation and input layer, not the source of truth.
- Gemini is a reasoning layer, not a mutation engine.
- All meaningful game-state changes pass through validated server commands.
- Every important mutation should generate a domain event.
- The project keeps dependencies lean and avoids premature complexity.

## Architectural core

The game follows a server-authoritative model:

- PostgreSQL/Supabase holds the authoritative world state.
- Unity reads and presents state, sends requests, and renders gameplay.
- The backend validates incoming commands and applies safe mutations.
- The AI orchestrator assembles context and can propose intents or dialogue but cannot directly mutate authoritative state.
- Domain events describe meaningful state changes for debugging, analytics, and future systems.

### Trust boundaries

- Unity: untrusted client
- Backend API / Edge Functions: trusted validation layer
- AI orchestrator: reasoning layer only
- PostgreSQL / Supabase: system of record
- Gemini: external reasoning model with no direct mutation rights

## Design philosophy

The repository enforces a disciplined architecture and a narrow development scope. The key design rules are:

1. The World Model is authoritative.
2. Unity is untrusted.
3. Gemini interprets the World Model and cannot mutate it directly.
4. Important mutations must pass through validated server commands.
5. Every meaningful mutation should produce a domain event.
6. Secrets must never be exposed to Unity.
7. Client-provided money, inventory, reputation, and progression are never trusted.
8. Keep dependencies minimal.
9. Prefer simple modular architecture over premature microservices.
10. Preserve existing functionality.
11. Do not rewrite unrelated systems.
12. Add tests for new business logic.
13. Keep API contracts explicit.
14. Use TypeScript for backend contracts.
15. Use C# for Unity gameplay code.
16. Use PostgreSQL constraints for critical invariants.
17. Use request IDs for idempotency.
18. Explain architectural changes before implementation.

## Repository layout

```text
naija-after-dark/
├── AGENTS.md
├── README.md
├── ai/
│   ├── evaluations/
│   ├── prompts/
│   └── schemas/
├── backend/
│   ├── ai-orchestrator/
│   └── game-api/
├── docs/
│   ├── ARCHITECTURE.md
│   ├── DEVELOPMENT.md
│   ├── GATE16_MOBILE_OPT.md
│   ├── GATES_7_10.md
│   ├── GATES_11_14.md
│   ├── GATES_15_20.md
│   ├── NEXT_ARCHITECTURE.md
│   ├── PHASE_A_COMMAND_ROUTER.md
│   └── WORLD-MODEL-SPEC.md
├── game/
│   └── unity/
├── supabase/
│   ├── config.toml
│   ├── functions/
│   ├── migrations/
│   ├── seed.sql
│   └── seed/
├── tools/
├── world-model/
│   ├── README.md
│   ├── schemas/
│   ├── types/
│   ├── mappings/
│   └── diagrams/
├── .gitignore
└── LICENSE (if added later)
```

## Key directories

### world-model/
Defines the authoritative schema contracts and the game-state model. This is the canonical description of the simulation state, including players, locations, relationships, and world properties.

### supabase/
Stores the actual PostgreSQL schema, migrations, configuration, seed data, and edge-function setup. This is the operational backend and the source of truth for live state.

### backend/game-api/
Handles validated game commands, server-side business logic, state mutation, and domain event emission. This is the command layer between client requests and the world model.

### backend/ai-orchestrator/
Contains the Gemini-backed interpretation layer, context assembly, and validation for AI-guided dialogue or NPC intent proposals.

### game/unity/
Contains the gameplay client. It renders the world, presents state, and sends commands to the backend; it does not own authoritative system state.

### ai/
Stores prompts, AI schemas, and evaluations used to generate and validate reasoning and dialogue behaviors.

### docs/
Contains architecture references, gate-specific documentation, future-phase documents, and development workflow notes.

## Current milestone status

The project documentation indicates the game is in a staged gate-based development cycle. The current documented milestone is:

- GATES 15–20: foundation verified on live DB

This includes the following progress markers:

- Gate 15: Rooms + presence (join/leave/pose verified)
- Gate 16: Mobile optimization checklist
- Gate 17: 10 locations + 5 careers
- Gate 18: Social opportunities
- Gate 19: World events + attendance
- Gate 20: Reactivity between attendance and nightlife/traffic

The project also outlines a next-phase roadmap:

- Phase A: command router live
- Phase B: Unity First Night vertical slice on device
- Phase C: Gemini Edge Function
- Phase D: contract hardening
- Phase E: realtime presence (optional)
- Phase F: Gate 16 mobile profiling

## Technology stack

- Game client: Unity (C#)
- Backend contracts: TypeScript
- Database: PostgreSQL via Supabase
- AI layer: Gemini (server-side only)
- Development tooling: Node.js, Supabase CLI, Git, GitHub CLI
- Optional tooling: Docker Desktop

## Local development workflow

The project emphasizes a disciplined solo-developer flow:

1. Define the schema.
2. Define the server command.
3. Define validation.
4. Define the domain event.
5. Implement the backend.
6. Implement the Unity client.
7. Add AI only if needed.
8. Test.
9. Commit.
10. Move to the next gate.

### Prerequisites

- Unity Hub + Unity LTS
- Android build support
- Git + GitHub CLI
- Node.js LTS
- Supabase CLI
- VS Code or Windsurf

Optional:

- Docker Desktop

### Supabase setup notes

The repo documents a project reference and a standard local setup flow:

```bash
supabase login
supabase link --project-ref unzfqrfyejkyisalzkhc
supabase db push
```

Keep service-role credentials on the server side only. Never put them into the Unity client.

## Security and trust model

The project’s design is intentionally strict about trust boundaries:

- Money, inventory, reputation, and progression are never accepted from the client as authoritative values.
- Secrets never leave the server environment.
- All state-changing commands are validated on the server.
- Critical invariants are enforced by PostgreSQL constraints.
- AI reasoning is kept out of the mutation path.

## Important documentation

To understand the project in depth, use these documents in order:

- `README.md` — project overview and rules
- `docs/ARCHITECTURE.md` — trust boundaries and architecture
- `docs/DEVELOPMENT.md` — development workflow and gate discipline
- `docs/NEXT_ARCHITECTURE.md` — roadmap and upcoming architecture
- `docs/PHASE_A_COMMAND_ROUTER.md` — command router details
- `docs/WORLD-MODEL-SPEC.md` — world model specification
- `world-model/README.md` — state ownership and entity map
- `backend/game-api/README.md` — command API overview
- `backend/ai-orchestrator/README.md` — AI orchestration flow

## Project vision

NAIJA AFTER DARK aims to create a polished vertical slice of a nightlife simulation in a Nigerian city context. The project is intentionally narrow and disciplined: prove the world model, verify command validation, validate gameplay loops, then expand carefully only when the foundation is stable.

The long-term direction is to build a playable social simulation where movement, money, social opportunities, nightlife events, and progression are all underpinned by a stable server-authoritative system.

## Contribution expectations

When contributing:

- respect the authoritative world model
- validate all state changes on the server
- avoid unnecessary dependencies or microservice proliferation
- keep contracts explicit
- add tests for business logic changes
- explain architectural changes before implementation

## Summary

NAIJA AFTER DARK is an architecture-first nightlife simulation project with a clear separation between authoritative state, client rendering, and AI interpretation. It is designed for disciplined incremental growth, with a strong emphasis on safe mutation flow, validation, and predictable gameplay state evolution.

This repository is best understood as a foundation for a playable vertical slice that prioritizes correctness and architecture before broad feature expansion.

## License

No project license is currently declared in the repository metadata. If you plan to distribute or reuse the project, confirm the intended license before publishing or sharing code.
