# NAAD AI Orchestrator (Gate 9)

Server-side Gemini interpretation layer.

## Rules

1. Gemini API key never leaves the server / Edge Function secrets.
2. Gemini **cannot** mutate wallets, locations, or memories directly.
3. Output must pass `validateDialogueResponse`.
4. Memory proposals go through `naad_commit_npc_memory` after validation.
5. Only Tunde is AI-backed in this gate; other NPCs use deterministic lines.

## Flow

```
Unity (user JWT)
  → Edge Function npc-dialogue
  → naad_npc_dialogue_context (Postgres)
  → Gemini
  → schema validate
  → response to Unity (proposals only)
  → optional: naad_commit_npc_memory (server RPC)
```

## Secrets

Set on the Edge Function / host:

- `GEMINI_API_KEY`
- `SUPABASE_URL` / `SUPABASE_ANON_KEY` (auto in Supabase functions)

## Test

```bash
npx --yes tsx backend/ai-orchestrator/src/validate.test.ts
```
