# NAIJA AFTER DARK

## Why this game?

Nightlife in Nigeria is vibrant, layered, and full of personality. From the energy of the streets to the pulse of clubs, beach gatherings, social circles, and career-driven routines, the world is alive with opportunities, tension, and momentum. NAIJA AFTER DARK takes that energy and turns it into a simulation-first experience where players navigate city life, build relationships, spend time in social spaces, and shape how their night unfolds.

This is not just a map with activities — it is designed around a disciplined world model and a server-authoritative architecture.

- The world state is treated as the source of truth.
- The player experience is built on top of that world rather than replacing it.
- The backend validates actions and enforces rules.
- AI enhances the experience without breaking the rules of the simulation.
- Every major moment is designed to feel meaningful, reactive, and connected.

## Gameplay vision

Imagine a night in motion:

- You arrive in a district and choose how to spend your evening.
- You meet people, build trust, and deepen social connections.
- You decide where to go, what to spend, and how to shape your reputation.
- The city reacts to energy, attendance, and activity.
- Events, nightlife patterns, and social opportunities create a living world.

This is a life-sim and nightlife sandbox built to feel vibrant, layered, and emergent.

## Core pillars

### 1. Social simulation

The city is full of people, routines, and relationships. Social opportunities, NPC interactions, and trust-building are central to the experience.

### 2. World-state authority

The project follows a strict model: the world model is authoritative. Game state is not allowed to drift from the server-backed truth.

### 3. Playable vertical slice

The team is intentionally building a tightly scoped foundation first. The focus is on proving the core loop and system integrity before broad expansion.

### 4. AI with boundaries

Gemini is used as a reasoning layer for interpretation and dialogue support, but it does not overwrite the authoritative state. The system keeps AI helpful without making the world unsafe or inconsistent.

## Architecture at a glance

NAIJA AFTER DARK is built around a clean trust model:

- Unity powers the player experience and rendering.
- The backend validates and applies gameplay commands.
- PostgreSQL/Supabase holds the live world state.
- AI interprets context and proposes dialogue or intent, but does not mutate the authoritative model directly.

This keeps the project stable, testable, and ready for future features.

## Current progress

The project has already reached a meaningful milestone in its staged development roadmap:

- Gates 15–20 foundation verified on live DB
- Verified social and nightlife systems such as rooms, presence, events, and reactivity
- Strong foundation for a first playable night experience

The roadmap continues toward a first-party mobile-friendly vertical slice and then deeper world-building.

## What is in the repo?

```text
na
... [truncated for brevity] ...

base migrations and Edge Functions live under `supabase/`; TypeScript contracts and AI orchestration are under `backend/`; schemas and architecture references are under `world-model/` and `docs/`.

Local provider environment setup is documented in [`docs/LOCAL_PROVIDER_CONFIGURATION.md`](docs/LOCAL_PROVIDER_CONFIGURATION.md). Credentials remain process environment values and are never stored in Unity assets or source control.

The active handoff checklist is [`docs/NEXT_PHASE_TASKS.md`](docs/NEXT_PHASE_TASKS.md); it separates Unity, staging Supabase, CI, and provider gates and records which actions require external installation or approval.

## AI provider status

The active NPC dialogue implementation uses Gemini through the Supabase Edge Function at [`supabase/functions/npc-dialogue/index.ts`](supabase/functions/npc-dialogue/index.ts). The backend orchestrator also targets Gemini. The OpenAI provider in [`game/unity/Assets/NAAD/AI/UnifiedAIClient.cs`](game/unity/Assets/NAAD/AI/UnifiedAIClient.cs) is currently a placeholder and must not be treated as a live integration.

If OpenAI is implemented later, choose a supported model before configuring prompt caching. Keep reusable instructions, schemas, and tool definitions stable and before dynamic conversation/world context. Record the API usage fields `input_tokens`, `input_tokens_details.cached_tokens`, and `input_tokens_details.cache_write_tokens`; latency-only measurements cannot establish cache savings. See the [official prompt-caching guide](https://developers.openai.com/api/docs/guides/prompt-caching) for model-specific minimum prefix lengths, cache keys, breakpoints, modes, and retention.

Current repository measurements include provider success/latency instrumentation and rough token estimates for the Gemini path. There are no measured OpenAI cache reads, writes, or input costs because no live OpenAI request is implemented.

## Local development outline

1. Review [`AGENTS.md`](AGENTS.md) and the relevant package or project documentation.
2. Use `game/unity/` for the main Unity client and keep generated Unity folders out of source control.
3. Use Supabase migrations and Edge Functions for server-side state and dialogue behavior.
4. Run focused tests or validation for the area changed; avoid production or paid-provider calls during local checks.
5. Check `git status` before handing off work and describe any unverified integration steps.

## Why it matters

NAIJA AFTER DARK is more than a game prototype — it is a disciplined attempt to build a dynamic nightlife life sim with a strong technical foundation. The project combines game design, world modeling, backend validation, and AI-assisted interaction into one cohesive vision.

It is designed for creators who want to build immersive experiences without sacrificing system integrity.

## The vision

We want to create a nightlife simulation that feels alive:

- full of movement
- full of social energy
- full of choice
- full of consequence
- grounded in a believable city rhythm

Every feature is being built with the long-term goal of making the world feel both fun and real.

## Join the journey

NAIJA AFTER DARK is a passion project and a serious technical prototype for a nightlife simulation that respects both gameplay and architecture.

If you are interested in indie game development, world modeling, backend systems, or AI-enhanced gameplay, this repo is where the foundation is being built.

## License

This repository does not currently declare a project license in the repo metadata. If you plan to distribute or reuse the project beyond local development, confirm the intended licensing status before publishing or sharing it.
## What makes it different?

This project is not just a map with activities — it is designed around a disciplined world model and a server-authoritative architecture.

- The world state is treated as the source of truth.
- The player experience is built on top of that world rather than replacing it.
- The backend validates actions and enforces rules.
- AI enhances the experience without breaking the rules of the simulation.
- Every major moment is designed to feel meaningful, reactive, and connected.

## Gameplay vision

Imagine a night in motion:

- You arrive in a district and choose how to spend your evening.
- You meet people, build trust, and deepen social connections.
- You decide where to go, what to spend, and how to shape your reputation.
- The city reacts to energy, attendance, and activity.
- Events, nightlife patterns, and social opportunities create a living world.

This is a life-sim and nightlife sandbox built to feel vibrant, layered, and emergent.

## Core pillars

### 1. Social simulation
The city is full of people, routines, and relationships. Social opportunities, NPC interactions, and trust-building are central to the experience.

### 2. World-state authority
The project follows a strict model: the world model is authoritative. Game state is not allowed to drift from the server-backed truth.

### 3. Playable vertical slice
The team is intentionally building a tightly scoped foundation first. The focus is on proving the core loop and system integrity before expanding the city or adding layers of complexity.

### 4. AI with boundaries
Gemini is used as a reasoning layer for interpretation and dialogue support, but it does not overwrite the authoritative state. The system keeps AI helpful without making the world unsafe or inconsistent.

## Architecture at a glance

NAIJA AFTER DARK is built around a clean trust model:

- Unity powers the player experience and rendering.
- The backend validates and applies gameplay commands.
- PostgreSQL/Supabase holds the live world state.
- AI interprets context and proposes dialogue or intent, but does not mutate the authoritative model directly.

This keeps the project stable, testable, and ready for future features.

## Current progress

The project has already reached a meaningful milestone in its staged development roadmap:

- Gates 15–20 foundation verified on live DB
- Verified social and nightlife systems such as rooms, presence, events, and reactivity
- Strong foundation for a first playable night experience

The roadmap continues toward a first-party mobile-friendly vertical slice and then deeper world-building.

## What is in the repo?

```text
naija-after-dark/
├── ai/
├── backend/
│   ├── ai-orchestrator/
│   └── game-api/
├── docs/
├── game/
│   └── unity/
├── supabase/
├── tools/
├── world-model/
├── README.md
├── AGENTS.md
└── .gitignore
```

## Development guide

The repository-wide contributor and agent rules are in [`AGENTS.md`](AGENTS.md). The most important operating rules are:

- The server-backed world model is authoritative.
- Unity requests and renders state; it does not bypass backend validation.
- AI produces dialogue and bounded proposals. State changes go through validated server commands.
- Secrets and service-role credentials remain server-side.
- Changes should include focused validation and documentation when contracts or workflows change.

The primary Unity project is `game/unity/`. Supabase migrations and Edge Functions live under `supabase/`; TypeScript contracts and AI orchestration are under `backend/`; schemas and architecture references are under `world-model/` and `docs/`.

Local provider environment setup is documented in [`docs/LOCAL_PROVIDER_CONFIGURATION.md`](docs/LOCAL_PROVIDER_CONFIGURATION.md). Credentials remain process environment values and are never stored in Unity assets or source control.

The active handoff checklist is [`docs/NEXT_PHASE_TASKS.md`](docs/NEXT_PHASE_TASKS.md); it separates Unity, staging Supabase, CI, and provider gates and records which actions require external installation or approval.

## AI provider status

The active NPC dialogue implementation uses Gemini through the Supabase Edge Function at [`supabase/functions/npc-dialogue/index.ts`](supabase/functions/npc-dialogue/index.ts). The backend orchestrator also targets Gemini. The OpenAI provider in [`game/unity/Assets/NAAD/AI/UnifiedAIClient.cs`](game/unity/Assets/NAAD/AI/UnifiedAIClient.cs) is currently a placeholder and must not be treated as a live integration.

If OpenAI is implemented later, choose a supported model before configuring prompt caching. Keep reusable instructions, schemas, and tool definitions stable and before dynamic conversation/world context. Record the API usage fields `input_tokens`, `input_tokens_details.cached_tokens`, and `input_tokens_details.cache_write_tokens`; latency-only measurements cannot establish cache savings. See the [official prompt-caching guide](https://developers.openai.com/api/docs/guides/prompt-caching) for model-specific minimum prefix lengths, cache keys, breakpoints, modes, and retention.

Current repository measurements include provider success/latency instrumentation and rough token estimates for the Gemini path. There are no measured OpenAI cache reads, writes, or input costs because no live OpenAI request is implemented.

## Local development outline

1. Review [`AGENTS.md`](AGENTS.md) and the relevant package or project documentation.
2. Use `game/unity/` for the main Unity client and keep generated Unity folders out of source control.
3. Use Supabase migrations and Edge Functions for server-side state and dialogue behavior.
4. Run focused tests or validation for the area changed; avoid production or paid-provider calls during local checks.
5. Check `git status` before handing off work and describe any unverified integration steps.

## Why it matters

NAIJA AFTER DARK is more than a game prototype — it is a disciplined attempt to build a dynamic nightlife life sim with a strong technical foundation. The project combines game design, world modeling, backend validation, and AI-assisted interaction into one cohesive vision.

It is designed for creators who want to build immersive experiences without sacrificing system integrity.

## The vision

We want to create a nightlife simulation that feels alive:

- full of movement
- full of social energy
- full of choice
- full of consequence
- grounded in a believable city rhythm

Every feature is being built with the long-term goal of making the world feel both fun and real.

## Join the journey

NAIJA AFTER DARK is a passion project and a serious technical prototype for a nightlife simulation that respects both gameplay and architecture.

If you are interested in indie game development, world modeling, backend systems, or AI-enhanced gameplay, this repo is where the foundation is being built.

## License

This repository does not currently declare a project license in the repo metadata. If you plan to distribute or reuse the project beyond local development, confirm the intended licensing status before publishing or sharing it.
