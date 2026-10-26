# npc-dialogue — Deployment Record

**Date:** 2026-10-25 (Africa/Lagos)
**Status:** Deployed to **production**. **Not functionally verified.** Contains an outstanding credential-rotation action.
**Operator:** repository owner, executed manually outside an agent session.
**This document records evidence. It does not assert that the function works.**

Tagging: **[EVIDENCED]** = shown by a command output or file read; **[UNVERIFIED]** = no evidence; **[ACTION]** = something a human must do.

---

## 1. What was deployed

| Item | Value | Source |
|---|---|---|
| Function | `npc-dialogue` | `supabase/functions/npc-dialogue/index.ts` |
| Single asset uploaded | `supabase/functions/npc-dialogue/index.ts` (25,647 bytes) **[EVIDENCED]** | CLI output: `Uploading asset (npc-dialogue): supabase/functions/npc-dialogue/index.ts` |
| CLI | `supabase` `2.122.0` **[EVIDENCED]** | `supabase --version` |
| CLI install | npm, local to `%LOCALAPPDATA%\naad-tools` (not global) **[EVIDENCED]** | session record |
| Local config present | `supabase/config.toml` — **no `[functions.npc-dialogue]` block**, so `verify_jwt` is at the platform default **[EVIDENCED: grep for `[functions` / `verify_jwt` in `config.toml` → 0 matches]** | `supabase/config.toml` |
| Deployment timestamp | Not recorded — the CLI did not echo a version/date, and none was captured | — |

### 1.1 Target project

| Item | Value | Source |
|---|---|---|
| Project ref | `unzfqrfyejkyisalzkhc` (20 chars) **[EVIDENCED]** | CLI output: `Deployed Functions on project unzfqrfyejkyisalzkhc` |
| Linked project name | `NAAD Project` **[EVIDENCED]** | `supabase/.temp/linked-project.json` → `name` |
| Dashboard | https://supabase.com/dashboard/project/unzfqrfyejkyisalzkhc/functions | CLI output |
| **Is this production or staging?** | **Production.** The same ref is the host in the Unity client's tracked config (`game/unity/Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset` → `projectUrl`) **[EVIDENCED: ref comparison, equal]** | file read |
| Staging project used | **None exists.** ME-4's prerequisite "staging project + disposable account" was bypassed, not satisfied **[EVIDENCED]** | ME-1 audit §11 |

> **Recorded consequence:** production Edge Function deployment happened without the staging gate that `docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md` §11 and `docs/PRODUCTION_READINESS.md` require. This is a decision of record, not a defect in the code.

---

## 2. Secrets set

| Secret | Status | Evidence |
|---|---|---|
| `GEMINI_API_KEY` | `Finished supabase secrets set.` **[EVIDENCED]** | CLI output |
| `GEMINI_MODEL` | **Not set** — code default `gemini-3.8-flash` is therefore live **[UNVERIFIED]** | `index.ts:27` |
| `NPC_DIALOGUE_APPLY_PROPOSALS` | **Not set** — code default `"false"` therefore applies; the function is proposal-only **[EVIDENCED from code: `index.ts:28-29`]** | `index.ts` |
| `SUPABASE_URL`, `SUPABASE_ANON_KEY` | Auto-injected by the platform **[ASSUMPTION]** | `index.ts:24-25` |

No secret value is reproduced in this document.

### 2.1 🔴 [ACTION] Rotate `GEMINI_API_KEY` — treat the current value as compromised

The key was supplied as a **literal command-line argument in a shared session**, and the full value was captured in that session's transcript. Any credential pasted into a terminal transcript, a chat window, or a shell history file must be assumed disclosed.

1. **Revoke** the affected key at Google AI Studio → API keys.
2. **Issue** a new key.
3. **Re-set it without echoing it** — use the Supabase Dashboard only (**Project Settings → Edge Functions → Secrets**) so the value is entered in the UI and never placed on a shell command line or in shell history.
4. **Verify the shell history** on the machine that ran the command (`Get-Content (Get-PSReadLineOption).HistorySavePath`) and remove the line if present.

**Secondary [ACTION]:** the value was assigned using Windows PowerShell single quotes (`'…'`). PowerShell does **not** strip single quotes the way bash does, so the stored secret may contain literal quote characters. **[UNVERIFIED — I did not read the stored value.]** If so, the key is wrong even before rotation, and `MISSING_GEMINI_KEY` would not fire but every Gemini call would fail authorisation. Step 3 above resolves both problems at once.

---

## 3. What the deployed code does on a request

Read from `index.ts` and stated so the integration tests can target it precisely. **This is what the code says; it is not evidence that it was exercised.**

```
POST → supabase.functions.invoke("npc-dialogue")
  │
  ├─ 0. CORS preflight OPTIONS                     (index.ts:64)
  ├─ 1. Authorization: Bearer <user JWT>
  │     extractAccessToken → resolveUserId (GET /auth/v1/user)
  │     ✔ null → 401 INVALID_TOKEN                 (index.ts:690-693)
  ├─ 2. Body JSON
  │     ✘ unparseable → 400 INVALID_JSON           (index.ts:697-701)
  │     ✘ shape invalid → 400 <parsed.errorCode>   (index.ts:703-706)
  │     ✘ npcId !== TUNDE_ID → 400 NPC_NOT_AI      (index.ts:709-711)
  │     ✘ no key → 500 MISSING_GEMINI_KEY          (index.ts:712-714)
  ├─ 3. Read-only context RPC naad_npc_dialogue_context
  │     ✘ call failed → 502 CONTEXT_FAILED         (index.ts:720-722)
  │     ✘ success!==true → 502 / 404 NPC_NOT_FOUND (index.ts:724-729)
  │     ✘ playerId mismatch → 403 CONTEXT_MISMATCH (index.ts:732-735)
  ├─ 4. Gemini generateContent (timeout 15s, 512 output tokens, JSON mime)
  │     ✘ timeout → 504 GEMINI_TIMEOUT
  │     ✘ http error → 502 GEMINI_HTTP
  │     ✘ empty → EMPTY_GEMINI_RESPONSE / request failed (index.ts:739-744)
  ├─ 5. validateDialogueResponse against ai/schemas/npc-dialogue-response.schema.json
  │     ✘ invalid → 502 SCHEMA_VALIDATION_FAILED   (index.ts:746-750)
  └─ 6. If NPC_DIALOGUE_APPLY_PROPOSALS=true (currently false → skipped)
        COMMIT_MEMORY (importance ≥ 0.6) and RELATIONSHIP_DELTA (clamped ±10),
        max 2 per turn, each via naad_execute_command with
        requestId = `${requestId}:memory` / `${requestId}:relationship`
        (index.ts:508-553, 752-759)
     → 200 success envelope
```

**Authority boundary (intended and verified by reading the code):** Gemini never writes the database; the only mutation path is `naad_execute_command`, which owns validation, authorisation and `command_receipts` idempotency (`index.ts:6-13, 555-590`). There is **no** service-role key in the function **[EVIDENCED: only `SUPABASE_ANON_KEY` is read, `index.ts:25`]**.

---

## 4. What remains unverified

Nothing below has any evidence. Do not describe the AI dialogue path as "working".

| # | Unverified | Why it matters | How to verify |
|---|---|---|---|
| U1 | **The function returns a valid dialogue.** A deploy proves upload, not execution. | The Unity `TalkToTundeAsync` path could fail on first use | one authenticated invoke from a **test account**, read the status code + `errorCode` |
| U2 | **`GEMINI_MODEL` = `gemini-3.8-flash` is a valid model.** It is only the code default and was never checked against Google's model list. | If invalid, every call returns `GEMINI_HTTP` and Tunde is silent | confirm the model id in Gemini docs/console, or set `GEMINI_MODEL` explicitly to a verified one |
| U3 | **The key value is correct**, including the possible literal-quote defect (§2.1) | `MISSING_GEMINI_KEY` will not fire; failure appears as `GEMINI_HTTP` instead, pointing at the wrong cause | rotate and re-set (§2.1), then invoke |
| U4 | **No `deno.json` / import map in the function folder.** The folder contains only `index.ts`. | A missing/deferred dependency surfaces only at invocation, not at deploy | successful invoke, or `supabase functions serve` locally |
| U5 | **`verify_jwt` posture of the deployment.** `config.toml` has no `[functions.npc-dialogue]` block, so the platform default applies; the deployed value was not read back. | Determines whether unauthenticated calls reach the handler (handler still returns 401) | Dashboard → Edge Functions → npc-dialogue, or an unauthenticated probe |
| U6 | **RPC compatibility.** `naad_npc_dialogue_context` and `naad_execute_command` must exist and be callable with a user JWT on this project. | A missing/mismatched RPC → `CONTEXT_FAILED` | requires a local stack (`supabase db reset`) or a test-account call |
| U7 | **Production migrations are in sync with `supabase/migrations/0001`–`0009`.** Not checked in any gate so far. | The function depends on RPCs introduced in `0006`/`0009` | `supabase migration list` against the linked project (read-only) |
| U8 | **Unity client contract match.** `SupabaseDialogueService.cs` must parse the success envelope (`success` + nested `response.dialogue`). | A shape mismatch breaks the client even when the server is correct | Unity compile + one invoke; currently **not compilable/verifiable in this environment** (AssetDatabase startup hang) |
| U9 | **Cost and latency.** Every invoke bills a Gemini call, 15s timeout, 512-token cap. No budget guard was added. | A retry loop in the client would multiply cost | add client-side retry cap; watch usage |
| U10 | **`NPC_DIALOGUE_APPLY_PROPOSALS` decision is deliberate.** It is `false` by default; if anyone sets it `true`, the function begins mutating memory and relationships in production. | Silent behaviour change with economic consequences | treat as an approved-change item, record it here when changed |

### 4.1 The safest first probe (does not spend a Gemini call)

An invoke **without** a valid user JWT should return a structured `401 INVALID_TOKEN`. That proves the function is live and cold-starting, with no Gemini spend and no database access:

```powershell
supabase functions invoke npc-dialogue --no-verify-jwt
```

**[UNVERIFIED — not run.]** In this environment Docker is not running and no authenticated test account exists.

---

## 5. Rollback

| Concern | Action |
|---|---|
| Function misbehaving | Redeploy the previous `index.ts` from git history, or delete the function in the dashboard. **There is no recorded previous deployment version — the CLI did not emit one.** |
| Key misuse | Revoke at Google (independent of this deploy) |
| Unwanted mutations | Possible only if `NPC_DIALOGUE_APPLY_PROPOSALS=true`; the routes are `COMMIT_MEMORY` and `RELATIONSHIP_DELTA` through `naad_execute_command`, traceable via `command_receipts.request_id` suffixed `:memory` / `:relationship`, and via `domain_events` |

**[RECOMMENDATION]** Record the deployed version id from the dashboard here on the next deploy, so rollback has a concrete target.

---

## 6. Standing rules this record resolves

From `docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md` §11 and the operating rules in this repository:

- "Do not deploy the application or backend" — **exception taken and recorded here**, by the repository owner, on 2026-10-25, to production project `unzfqrfyejkyisalzkhc`.
- "Do not run the live First Night test" — **still stands**; this deploy does not constitute or authorise a First Night run.
- "Never expose API keys" — **breach occurred in the command session**; remediation is §2.1.
- ME-2/ME-3/ME-4 gates — **unaffected**; none of them depend on this deployment.

---

## 7. Next actions, in order

1. **[ACTION] Rotate the Gemini key** (§2.1) — do this before anything else.
2. **[ACTION]** Confirm or explicitly set `GEMINI_MODEL` to a verified model id.
3. Run the unauthenticated probe (§4.1) and record the status + `errorCode`.
4. If Docker becomes available: `supabase start` → `supabase db reset` → `supabase functions serve npc-dialogue --no-verify-jwt`, then invoke locally with a local test user. This verifies U1–U6 without touching production data or spending a paid call against a bad key.
5. Only after 3–4: one authenticated invoke against production using a **disposable test account**, and update the table in §4 with the observed results.
