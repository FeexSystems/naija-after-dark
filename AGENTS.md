# NAIJA AFTER DARK — Agent Guide

This repository contains the NAIJA AFTER DARK nightlife life-simulation prototype. It combines a Unity client, Supabase/PostgreSQL services, server-side dialogue orchestration, world-model schemas, and asset-production tools.

## Project principles

- Preserve the authoritative-world model. The backend validates and applies gameplay state changes; the client displays and requests them.
- Treat AI output as a proposal. AI may interpret context and generate dialogue, but it must not directly mutate money, inventory, relationships, memories, time, or other authoritative state.
- Keep secrets server-side. Never commit API keys, service-role credentials, production JWTs, or copied `.env` files. Unity builds must not contain privileged backend credentials.
- Prefer the smallest change that satisfies the request. Preserve existing conventions and unrelated working-tree changes.
- Do not fabricate responses, state, test results, or integration status. Mark placeholders and estimates clearly.

## Repository map

- `game/unity/` — primary Unity project and runtime client.
- `naad-fntest/` — secondary Unity test/fixture project; do not assume it is the active game project.
- `supabase/` — database migrations, configuration, and Edge Functions.
- `backend/game-api/` — TypeScript contracts and API-facing models.
- `backend/ai-orchestrator/` — server-side dialogue validation/orchestration tests and types.
- `world-model/` — JSON schemas, mappings, and diagrams for authoritative state.
- `ai/` — AI schemas and prompt assets.
- `Gameplay/` — shared/legacy dialogue gameplay scripts.
- `docs/` — architecture, gates, development, and production documentation.
- `tools/` — Blender, Unity asset, DCC bridge, and verification utilities.

## Change workflow

1. Read the nearest `AGENTS.md` before changing files. A deeper guide overrides this one for its subtree.
2. Inspect the smallest relevant set of files before editing. Check `git status` and preserve unrelated changes.
3. Follow existing naming, serialization, logging, and error-envelope conventions.
4. Keep generated Unity folders such as `Library/`, `Temp/`, `Obj/`, and build output out of commits.
5. Update documentation when behavior, contracts, setup, or architecture changes.
6. Run focused validation appropriate to the change, then report exactly what ran and what was not run.

## Unity guidance

- The primary project is `game/unity/`; inspect `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt` before Unity-specific work.
- Runtime code belongs under `game/unity/Assets/`. Editor-only code belongs in an `Editor` folder.
- Do not edit Unity-generated cache folders or package cache copies.
- Preserve `.meta` files when moving or creating Unity assets.
- For scene or asset changes, verify through the Unity Editor or the project’s documented command-line workflow when available.
- Avoid embedding secrets in ScriptableObjects, scenes, prefabs, or serialized MonoBehaviour fields.

## Backend and database guidance

- Database migrations in `supabase/migrations/` are append-only. Do not rewrite an already-applied migration.
- State-changing requests must use the existing validated command/router path and retain request IDs/idempotency behavior.
- Maintain the documented response envelopes consumed by Unity services.
- Validate external and AI-produced data at the server boundary; clamp, bound, and reject invalid values before persistence.
- Do not use a service-role key from Unity or an untrusted client.

## AI and OpenAI guidance

- The active NPC dialogue path currently calls Gemini from `supabase/functions/npc-dialogue/index.ts`; `backend/ai-orchestrator/` also targets Gemini.
- `game/unity/Assets/NAAD/AI/UnifiedAIClient.cs` contains an OpenAI provider placeholder, not a live OpenAI integration. Do not describe it as active until a real request path is implemented and tested.
- Keep stable system instructions and output schemas separate from dynamic player/world context where the provider supports prompt caching.
- For OpenAI work, select a supported model first. Apply only that model’s documented cache controls. Capture `input_tokens`, `cached_tokens`, and `cache_write_tokens` before claiming savings.
- Do not run paid API calls, create credentials, or change production configuration without explicit approval.
- Keep prompts/schema versions explicit so behavior changes and cache invalidation are traceable.

## Testing and validation

- TypeScript changes: run the relevant package test/typecheck commands documented by that package.
- Supabase functions: run local validation/tests where available; do not call production services by default.
- Unity changes: compile/test through the Unity project workflow; check console errors for runtime/editor changes.
- Schema changes: validate representative fixtures against the changed schema and inspect affected contracts.
- Documentation-only changes: verify links, paths, commands, and claims against the repository.

## Commit and review expectations

- Keep commits focused and explain behavior changes in user-facing terms.
- Call out migrations, security implications, model/provider assumptions, and any unverified steps.
- Never include secrets, large generated artifacts, Unity caches, or unrelated formatting churn.
