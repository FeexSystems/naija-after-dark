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

## Type Check

```bash
npm run typecheck
```

**Note**: This package is designed to run in Deno (Supabase Edge Functions). The `typecheck` script validates TypeScript types, but the actual runtime is Deno, not Node.js. The `orchestrator.ts` file uses Deno APIs (Deno.env.get) which are not available in Node.js.

## Test

```bash
npm run test
```

## Development

This package uses npm workspaces. From the backend root:

```bash
npm install
npm run typecheck
npm run test
```
