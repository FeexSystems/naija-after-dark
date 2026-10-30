# Phase C — AI dialogue live path

**Status:** Client + Edge Function code on main / this branch. Live deploy needs `GEMINI_API_KEY`.

## Trust boundary (non-negotiable)

| Actor | Role |
|-------|------|
| Unity | Untrusted. Calls `functions/v1/npc-dialogue` with player JWT only. |
| `npc-dialogue` Edge Function | Loads context via RPC, calls Gemini, **validates JSON**, returns proposal. |
| Gemini | Interpretation only. **Never** holds DB credentials. |
| Postgres | Authoritative. Memory / relationship only via `naad_execute_command` when `NPC_DIALOGUE_APPLY_PROPOSALS=true`. |

## Deploy (operator)

```bash
# From repo root, linked to project unzfqrfyejkyisalzkhc
supabase secrets set GEMINI_API_KEY=YOUR_KEY
# optional
supabase secrets set GEMINI_MODEL=gemini-2.0-flash
# default false = proposal-only; true applies COMMIT_MEMORY / RELATIONSHIP_DELTA via router
supabase secrets set NPC_DIALOGUE_APPLY_PROPOSALS=false

supabase functions deploy npc-dialogue
```

Verify:

```bash
curl -sS -X POST "$SUPABASE_URL/functions/v1/npc-dialogue" \
  -H "Authorization: Bearer $PLAYER_JWT" \
  -H "apikey: $ANON_KEY" \
  -H "Content-Type: application/json" \
  -d '{"requestId":"phase-c-1","npcId":"e1111111-1111-1111-1111-111111111101","playerMessage":"Tunde, how far?"}'
```

Expect `success: true` and nested `response.dialogue`.

## Unity path

1. `NAADApplicationRoot` wires `SupabaseDialogueService` as `IDialogueService`.
2. `FirstNightController.TalkToTundeAsync(message)` → Edge Function.
3. First Night HUD: **Talk to Tunde (AI)**.

Deterministic NPCs still use `GreetAsync` → `naad_get_npc_greeting` (no Gemini).

## Files

- `supabase/functions/npc-dialogue/index.ts`
- `ai/schemas/npc-dialogue-response.schema.json`
- `game/unity/Assets/NAAD/Networking/Supabase/SupabaseDialogueService.cs`
- `game/unity/Assets/NAAD/Gameplay/FirstNight/FirstNightController.cs`

## Pass criteria

1. Authenticated player JWT reaches `npc-dialogue`.
2. Gemini key never appears in Unity builds or player-facing logs.
3. Invalid Gemini JSON fails closed (`SCHEMA_VALIDATION_FAILED` / `INVALID_JSON`).
4. Optional memory only when importance ≥ threshold and router accepts `COMMIT_MEMORY`.
