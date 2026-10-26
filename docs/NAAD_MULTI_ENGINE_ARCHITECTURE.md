# NAAD Multi-Engine Architecture — Decision Record (ME-1)

**Status:** ME-1 complete (audit). ME-2 **not started**.
**Date:** 2026-10-25 (Africa/Lagos)
**Scope:** Read-only repository audit plus this document. No Unity code, migration, backend, contract, lockfile or workflow was modified.
**Decision:** Unity stays the primary game client. The World Model stays engine-neutral. One experimental engine (Godot) may be evaluated later, in isolation. No second engine was created in this gate.

Every statement below is tagged:

- **[VERIFIED]** — observed directly in this checkout during this audit (command output or file read).
- **[ASSUMPTION]** — inferred, not proven by this audit.
- **[RECOMMENDATION]** — proposed work, not implemented.

---

## 1. Verified baseline

| Check | Observed | Evidence |
|---|---|---|
| Branch | `chore/unity-mcp-scaffolding-and-gitignore` | `git rev-parse --abbrev-ref HEAD` |
| Commit | `93d076bf6f81f7d21afb1c717eafa073d027fe26` | `git rev-parse HEAD` |
| Working tree | **Dirty: 1,376 entries** — ~200 tracked deletions in `game/unity/Assets/LLMUnity/`, plus untracked NAAD sources, animation `.meta` files and tool scripts | `git status --porcelain=v1 -uall \| Measure-Object -Line` |
| Local branches | `main`, `docs/naad-baseline-audit`, `fix/phase1-package-resolution`, current branch | `git branch -a` |
| Remote branches | `origin/main`, `origin/chore/phase1-pre-edit-decision-record`, `origin/chore/unity-mcp-scaffolding-and-gitignore`, `origin/cline/febyxe77`, `origin/docs/naad-baseline-audit`, `origin/feat/audit-gate-recovery-2026-10-14`, `origin/fix/phase1-package-resolution` | `git branch -a` |
| Unity editor (working tree) | **`6000.6.5f1`** (`pending-revision`) — a **local uncommitted downgrade** | `game/unity/ProjectSettings/ProjectVersion.txt` + `git diff` |
| Unity editor (committed at `93d076b`) | **`6000.7.0b4`** (`9debfb8362db`) | `git diff -- game/unity/ProjectSettings/ProjectVersion.txt` |
| Node | `v24.15.0` | `node --version` |

### 1.1 Baseline conflicts with the prompt (must be read before ME-2) **[VERIFIED]**

The master prompt asserts premises that this checkout does not support. Do not build ME-2 on them; two of the three are *working-tree* facts, not repository facts.

1. **Unity version.** Committed at `93d076b` the project is **`6000.7.0b4`**, which matches the prompt. The *working tree* carries an uncommitted downgrade of `ProjectVersion.txt` to **`6000.6.5f1`** (`pending-revision`):

   ```diff
   -m_EditorVersion: 6000.7.0b4
   -m_EditorVersionWithRevision: 6000.7.0b4 (9debfb8362db)
   +m_EditorVersion: 6000.6.5f1
   +m_EditorVersionWithRevision: 6000.6.5f1 (pending-revision)
   ```

   So `6000.7.0b4` is the repository's real version and `6000.6.5f1` is **local, uncommitted experiment state that must not be committed or built on**. The CI pin in `naad-unity.yml` is `6000.6.5f1` (§1.1 item 3), so the workflow currently agrees with the *local* state, not the repository. `docs/NAAD_BASELINE_AUDIT.md` records `6000.7.0b4` plus a **local Unity Editor startup hang** (AssetDatabase refresh stalls; `Library/PackageCache` watcher `Access is denied` / `InternalBufferOverflowException`). No local Unity verification exists at either version. This gate changes nothing here, as instructed.
2. **PR #11.** The prompt says "do not merge PR #11". `docs/NAAD_BASELINE_AUDIT.md` records PR #11 as **already closed and merged**. Nothing in this gate touched it. **[RECOMMENDATION]** Confirm PR #11 status on GitHub before restating the constraint; the live constraint is "do not let this work depend on PR #11's outcome", not "don't merge it".
3. **CI Unity pin.** `.github/workflows/naad-unity.yml` pins `unityVersion: 6000.6.5f1` in **six** `game-ci/unity-builder@v4` jobs — i.e. it matches the uncommitted local downgrade and **contradicts the committed `6000.7.0b4`**. `.github/workflows/naad-unity.yml` line 51 itself says "Keep this aligned with ProjectSettings/ProjectVersion.txt", which is currently only true for the dirty working tree. No workflow was edited here; this is a **pre-existing inconsistency to fix separately**, not part of ME-1.

### 1.2 Uncommitted work that a new branch must not disturb **[VERIFIED]**

| Path | State | Why it matters |
|---|---|---|
| `game/unity/ProjectSettings/ProjectVersion.txt` | **modified** — `6000.7.0b4` → `6000.6.5f1` | A local editor downgrade. **Never stage this.** It also makes the CI pin look correct when it is not (§1.1 item 3). |
| `game/unity/Packages/packages-lock.json` | modified (`378` lines changed) | Package graph churn, presumably paired with the downgrade. **Not verified or touched.** |
| `game/unity/Assets/LLMUnity/**` | ~200 tracked files **deleted** in the working tree | A vendored LLM runtime removed locally. Committing this here would be an unrelated dependency change. **Leave untouched.** |
| `game/unity/Assets/NAAD/Animation/Animations/**` (49 tracked `.fbx`, Locomotion / Phone / Social) | **deleted** in the working tree | The binary animation assets are gone locally while their `.meta` files are untracked additions. A `git add -A` would delete the assets and add orphaned metadata. **Leave untouched.** |
| `NAAD/**` C# touched in the working tree (`FirstNightController.cs`, `MockCommandService.cs`, `ICommandService.cs`, `TravelCommand.cs`, stubs, editor test runners) | modified | Not reviewed for content in this audit. They must not be described as “the current committed code” — the committed versions are what any contract check should be compared against. |
| `backend/game-api/src/contracts/*`, `world-model/**`, `supabase/**`, `ai/**` | **no diff** | The audit confirms these were not modified; contract work in ME-2 therefore starts from committed content |
| `game/unity/Assets/NAAD/Networking/CommandRequest.cs` | **untracked** | Not tracked by git (`git ls-files --error-unmatch` fails), yet `CommandRequest` is used by `MockCommandService`. **[RECOMMENDATION]** Never `git add -A`; stage per file. Also verify whether this file is intended to exist before any Unity compile claim. |
| `game/unity/Assets/NAAD/Networking/Commands/GameCommandTypes.cs` | untracked | Same risk. |
| `game/unity/Assets/NAAD/Networking/Models/DomainModels.cs` | untracked | Same risk. |
| `game/unity/Assets/NAAD/Editor/TempFirstNightLiveTest.cs` | untracked | A live-mutation test driver. Must not be wired into CI or run. |
| `game/unity/Assets/NAAD/**/*.fbx.meta` (~60) | untracked | Animation import metadata, unrelated. |
| `tools/mixamo_download.js`, `tools/Untitled-1.txt` | untracked | Local scratch. |
| `.codex/`, `.naad-authtest/`, `.venv/`, `PRP_Bed_ND,PRP_TV_ND/` | untracked/ignored dirs | Local inputs. |

**Branch decision for this work** (see §12): the current branch already carries unrelated tracked deletions. This gate produced only this documentation file, so it can be committed on a dedicated branch taken from `93d076b` **without** the LLMUnity deletions. **[RECOMMENDATION]** ME-2 should use its own branch, e.g. `feat/me2-engine-neutral-contracts`, also from `93d076b`, and must not repurpose the current branch.

### 1.3 Verification actually executed in this gate

| Check | Command | Result |
|---|---|---|
| Game-api contract test | `node --experimental-strip-types backend/game-api/src/contracts/contracts.test.ts` | **passed** — output `contracts.test.ts: ok`, exit 0 |
| AI orchestrator validation test | `node --experimental-strip-types backend/ai-orchestrator/src/validate.test.ts` | **passed** — output `validate.test.ts: ok`, exit 0 |
| Unity compile / package resolution / Play Mode | not run | No evidence. Blocked by the recorded editor-startup issue; also out of scope for this gate |
| Supabase CLI / migrations applied | not run | No local Supabase project was started |
| Live First Night test | **not run** | Forbidden |
| Deploy / merge / push | **not run** | Forbidden |

These are the *only* two green checks. Both are zero-dependency scripts run with Node's type stripping; **there is no `package.json` or `tsconfig.json` in `backend/game-api` or `backend/ai-orchestrator`** **[VERIFIED]**, so there is no `npm test` or `tsc` target today.

---

## 2. Current-state findings

### 2.1 Where the authoritative state actually lives **[VERIFIED]**

Nine append-only migrations in `supabase/migrations/` (`0001`–`0009`):

| Migration | Creates |
|---|---|
| `0001_initial_world_model.sql` | `players`, `wallets`, `locations`, `npcs`, `relationships`, `world_state`; RLS enabled on all six |
| `0002_auth_player_bootstrap.sql` | `handle_new_user()` trigger → auto-provisions player + wallet on `auth.users` insert |
| `0003_command_system_travel.sql` | `player_locations`, `command_receipts` (PK `request_id`), `domain_events`, `naad_travel()` |
| `0004_world_time.sql` | `naad_get_world_clock()`, `naad_advance_world_time()`, `naad_set_world_period()`, period helpers |
| `0005_night_district_world.sql` | `districts`, `businesses`, `items`; read policies for authenticated users |
| `0006_dialogue_memory_economy.sql` | `npc_dialogue_lines`, `npc_memories`, `wallet_transactions`, `naad_buy()`, `naad_commit_npc_memory()`, `naad_npc_dialogue_context()` |
| `0007_economy_relationships_phone_first_night.sql` | `price_list`, `naad_get_wallet()`, `naad_spend()`, `naad_apply_relationship_delta()`, `messages`, `naad_list_messages()`, `naad_seed_tunde_beach_invite()` |
| `0008_multiplayer_content_events_reactivity.sql` | multiplayer/content/events/reactivity objects (not line-by-line audited here) |
| `0009_execute_command_router.sql` | `naad_execute_command(p_request_id, p_type, p_payload)` — the single mutation entry point |

The trust model is stated twice and is consistent between the two: `world-model/README.md` ("Unity never owns money, inventory, reputation, or progression") and `docs/NEXT_ARCHITECTURE.md` ("Postgres authoritative, Unity untrusted, Gemini proposals-only") **[VERIFIED]**.

### 2.2 The command path (this is the reusable backbone) **[VERIFIED]**

Read of `supabase/migrations/0009_execute_command_router.sql` (all lines), `0003_command_system_travel.sql` (lines 60–264):

```
client (JWT)  →  POST /rest/v1/rpc/naad_execute_command
                { p_request_id, p_type, p_payload }
              →  security definer function, auth.uid() required
              →  dispatch on upper(trim(p_type)) to a named handler
              →  handler: idempotency check on command_receipts.request_id
                        → validate preconditions
                        → mutate authoritative tables
                        → insert domain_events row
                        → insert command_receipts row
                        → return { requestId, success, errorCode, errorMessage, payload }
```

Fourteen command types dispatch: `TRAVEL`, `SPEND`, `BUY`, `START_NIGHT`, `COMPLETE_NIGHT`, `PHONE_REPLY`, `JOIN_ROOM`, `LEAVE_ROOM`, `ATTEND_EVENT`, `RELATIONSHIP_DELTA`, `SET_CAREER`, `ACCEPT_OPPORTUNITY`, `COMMIT_MEMORY`, `SET_WORLD_PERIOD`. `BUY` is an alias path that falls back to `naad_buy` if `sku` is absent. Unknown types return `UNSUPPORTED_COMMAND`; any exception is caught and returned as `COMMAND_EXCEPTION` with `SQLERRM`.

This envelope is already the de-facto cross-engine contract. **It is a result of the router, not a versioned spec.** That gap is the whole of ME-2.

### 2.3 Existing contract artifacts (all of them) **[VERIFIED]**

| Artifact | Format | Coverage | Tested? |
|---|---|
| `world-model/schemas/*.schema.json` (21 files) | JSON Schema Draft 2020-12 | Single-entity shapes: player, wallet, inventory, relationship, location, npc, npc-memory, world-state, event, … | **No test harness exists.** No JSON Schema validator is installed. |
| `world-model/types/index.ts` | TypeScript interfaces | Mirrors the schemas **[ASSUMPTION: hand-maintained; no generator found]** | No |
| `backend/game-api/src/contracts/{commands,world-clock,economy,phone,multiplayer}.ts` | TypeScript | Command registry, payload types, response envelope, location IDs, prices, period bounds | `contracts.test.ts` — **passes** |
| `game/unity/Assets/NAAD/Networking/Commands/GameCommandTypes.cs` + `DomainModels.cs` | C# | Unity mirror of the command vocabulary | Untracked; no compile evidence |
| `ai/schemas/npc-dialogue-response.schema.json`, `backend/ai-orchestrator/src/validate.ts` | JSON Schema + TS validator | AI output proposals | `validate.test.ts` — **passes** |

**There are three parallel copies of the vocabulary** (Postgres literals, TypeScript, C#) with **one** automated agreement check (`contracts.test.ts` pins prices, period bounds, location suffixes and the command registry — but it reads only the TS side and hard-codes the expected values, so it detects TS edits, not SQL drift) **[VERIFIED]**. This is the real multi-engine risk: not "will Godot work", but "will four clients agree with Postgres".

### 2.4 Client-side duplication and placeholders **[VERIFIED]**

Read: `NAADApplicationRoot.cs` (all), `MockCommandService.cs` (1–120, 390–429), `SupabaseCommandService.cs` (grep), `SupabasePhoneService.cs` (grep), `SupabaseConfig.cs` (all), `Stubs/*` (grep).

| Finding | Location | Severity for multi-engine |
|---|---|---|
| `MockCommandService` **implements economy rules**: skips to zero balance, `INSUFFICIENT_FUNDS` message, price table via `GetSkuPrice`, `balanceAfter` computation | `MockCommandService.cs` 398–424 | **High.** It is a second economy implementation. Its comment ("Simulates authoritative server behavior") invites re-implementing that logic in Godot. **Do not.** |
| Mock prices are hard-coded and **differ from the server**: `SUYA_PLATE` 5,000 / `CLUB_TICKET` 20,000 in TS+SQL, but the mock spends from a 50,000 starting balance with its own table | `MockCommandService.cs` 411, `MockPlayerService.cs` 50 | High — a mock that answers differently from the server teaches the wrong contract |
| Mock/mock-auth starting balance 50,000; `StubPhoneService.GetBalanceAsync` returns 100,000 | `MockPlayerService.cs` 50, `MockAuthenticationService.cs` 46, `StubPhoneService.cs` 60 | Medium — two different fake realities inside Unity today |
| Two stub families: `Stub*` (no-op, always true) and `Stubs/Mock*` (in-memory, validates) | `NAAD/Networking/Stub*.cs`, `NAAD/Networking/Stubs/Mock*.cs` | Medium — no single "dev backend" story |
| `SupabaseConfig` is a `ScriptableObject` holding `projectUrl` + `anonKey`; an `.asset` in `Resources/` is **tracked and contains a 208-character populated anon key** | `Config/SupabaseConfig.cs`, `Config/Resources/SupabaseConfig.asset` | Low (anon key is public by design and `IsConfigured` explicitly rejects `service_role`), but **do not copy this asset or pattern into any new engine project**, and never add the project URL/keys to a second engine's `.env` without the same client-only rule |
| Naive hand-rolled JSON parsing (`NaadJson`, string-scan) in Unity services | `Networking/Supabase/NaadJson.cs` | Low for correctness, **high for portability**: a new engine must not copy string-scan parsing; it should parse against the ME-2 schema |
| `[ASSUMPTION]` `game/unity/Assets/NAAD/AI/UnifiedAIClient.cs` OpenAI placeholder (per `AGENTS.md`) | — | Not re-verified in this gate; the active AI path is the `npc-dialogue` Edge Function → Gemini |

### 2.5 AI path **[VERIFIED]**

`backend/ai-orchestrator/README.md` and `supabase/functions/npc-dialogue/` (only Edge Function in the repo):

```
Unity (user JWT) → Edge Function npc-dialogue → naad_npc_dialogue_context() → Gemini
→ validateDialogueResponse → proposals only → client may then call naad_commit_npc_memory
```

Gemini never writes tables; memory proposals go through a validated RPC. This is already engine-neutral **in principle**, because the client only supplies a JWT and a player message. Nothing in the AI path needs to change for a second engine.

### 2.6 Repository layout: no place for a second game **[VERIFIED]**

Top level: `ai/`, `backend/`, `docs/`, `game/`, `Gameplay/`, `naad-fntest/`, `supabase/`, `tools/`, `world-model/`, `PRP_Bed_ND,PRP_TV_ND/`, `.github/`, root markdown files.

- **No `experiences/`, `prototypes/`, or `games/` directory exists.** The prompt's candidate path `experiences/godot/nightlife-mini-game/` would introduce a new top-level convention. **[RECOMMENDATION]** Prefer `game/godot/nightlife-mini-game/` so all game clients live under the existing `game/` parent, and document the convention rather than adding a fourth top-level bucket.
- `Gameplay/` holds **legacy shared dialogue scripts** (`DialogueManager.cs`, `DialogueBubble.cs`, condition classes) that do **not** implement the current command path. They are a trap for a new engine: they look like shared gameplay but are pre-command-bus. **[RECOMMENDATION]** Mark `Gameplay/` as legacy in `README.md`; do not reuse it as the "shared" layer.
- `naad-fntest/` is a second Unity project (per `AGENTS.md`: a test/fixture project, not the game). It is **not** a second engine and must not be presented as one.
- 1,901 tracked files under `game/unity/Assets/NAAD/`.

### 2.7 Test inventory **[VERIFIED]**

| Layer | Exists | Runs today |
|---|---|---|
| TS command/AI contracts | `contracts.test.ts`, `validate.test.ts` | **Yes, both green** |
| World Model JSON Schemas | 21 schemas | **No validator, no fixtures, no test** |
| SQL handlers | 9 migrations | **No SQL test harness in repo** |
| Unity EditMode/PlayMode tests | editor scripts exist (`AutomatedTestRunner.cs`, verifiers) | **No green run in this checkout** |
| CI | `.github/workflows/naad-unity.yml` — 6 Unity jobs + opt-in live First Night job (`if: workflow_dispatch && inputs.run_live_first_night_test`) | Not run in this gate; live job correctly gated behind a manual dispatch flag |

### 2.8 Blockers carried forward **[VERIFIED unless marked]**

1. Unity Editor cannot complete AssetDatabase startup in this environment (recorded in `docs/NAAD_BASELINE_AUDIT.md`; re-attempted at least five ways: clean cache, isolated copy, `-noUpm`, `-disableDirectoryMonitoring`, CLI). **No Unity change can be claimed verified.**
2. Working tree is dirty with 1,376 entries including unrelated tracked deletions. Any broad `git add` would sweep in the LLMUnity removal.
3. Three untracked C# files are required by tracked C# code (`CommandRequest.cs`), so the tracked tree **may not compile as committed** **[ASSUMPTION — cannot compile to confirm]**.
4. No `package.json`/`tsconfig.json` in the backend packages — no typecheck, no test runner, no single entry command.
5. There is no JSON Schema validator available locally for `world-model/schemas/`.
6. Version and PR-#11 premises in the prompt disagree with this checkout (§1.1).

---

## 3. Proposed architecture

Unchanged from the decision under review, with two concrete sharpenings: the shared layer is the **existing Postgres router**, and the second engine talks **only** through it.

```
                     Shared World Model (Postgres, migrations 0001–0009)
                                      │
                         naad_* read RPCs   |   naad_execute_command (mutations)
                                      │
                          ┌───────────┴────────────┐
                          │                        │
             Unity client (primary)      Godot mini-game (experiment)
             SupabaseAuthService         HTTP + JWT + JSON only
             SupabaseCommandService      no DB client, no service_role
                          │                        │
                          └──► contracts/naad/v1 ◄─┘   (read-only spec, generated or checked)
```

Two design rules that differ from the prompt's sketch:

- **[RECOMMENDATION] No new transport.** Do not build an "authenticated API layer" in front of PostgREST just for Godot. PostgREST + JWT + RLS **is** the authenticated API layer; it is already what Unity calls (`/rest/v1/rpc/naad_execute_command`). A new adapter is justified only if a concrete need (CORS, rate limits, response shaping) appears.
- **[RECOMMENDATION] Contracts are a *specification checked against* implementation, not a fourth hand-maintained copy.** The repo already fails at keeping three copies honest. ME-2 must implement a check that the TS types, the SQL dispatch table/error codes and the JSON Schema fixtures agree, otherwise it adds drift, not safety.

---

## 4. Engine responsibility matrix

| Concern | Postgres | Unity (primary) | Godot (experiment) | Web (later) |
|---|---|---|
| Wallet balance / transactions | **owns** | display from RPC only | display from mock or RPC only | display only |
| Inventory / items | **owns** | display | display only | display only |
| Relationship scores | **owns** | display | display only | display only |
| Player location | **owns** (`player_locations`) | display; requests `TRAVEL` | display; requests `TRAVEL` | display |
| World clock / weather | **owns** | interpolate presentation | interpolate presentation | expose |
| NPC memory | **owns** | never writes | absent | absent |
| Command validation, idempotency, events | **owns** | sends `request_id` | sends `request_id` | sends `request_id` |
| 3D city, movement, animation, scenes | — | **owns** | tiny room only | — |
| UI / input / camera | — | owns | owns (own implementation) | owns |
| Price table | **owns** (`price_list`) | reads/displays | **never hard-codes** | reads |
| Mock/dev backend | — | `Stub*` (no-op) only | local fixture adapter, **labelled MOCK** | — |

**Rule:** a client may hold a *display copy* and a *request*; it may never hold a *decision*.

---

## 5. Systems that must be reused, not rebuilt

| Reuse | Do not create |
|---|---|
| `naad_execute_command` + 14 handlers (`0009`) | a second command bus, GraphQL layer, or per-engine mutation endpoints |
| `command_receipts` idempotency | a client-side "already sent" dedupe as the only guard |
| `domain_events` | a second event log |
| `naad_get_wallet`, `naad_list_messages`, `naad_get_world_clock`, `naad_get_relationship`, `naad_npc_dialogue_context` | new read RPCs that duplicate these |
| `price_list` / `PRICES` | prices in a new engine's source |
| `world-model/schemas/*.json` (JSON Schema 2020-12) | a new schema dialect or Protobuf/MessagePack layer |
| `ai/schemas/npc-dialogue-response.schema.json` + `validate.ts` | per-engine AI parsing |
| Supabase Auth (email/password, JWT, RLS) | a Godot account system |
| `NaadJson`-equivalent **replaced, not ported** — new engines parse against ME-2 schemas | a copied string-scan parser |

---

## 6. Security and trust boundaries

**[VERIFIED]** controls already in place:

- RLS enabled on every gameplay table; `wallet_transactions`, `messages`, `command_receipts`, `domain_events`, `npc_memories` readable only by their owner.
- `0001` explicitly adds no INSERT/UPDATE/DELETE policy for wallets to `authenticated`; service role bypasses RLS for server-side handlers only.
- All command functions are `security definer` with `set search_path = public`, `revoke all … from public`, `grant execute … to authenticated`.
- `SupabaseConfig.IsConfigured` rejects any anon key containing `service_role`.
- Gemini key only in Edge Function secrets.

**[RECOMMENDATION] additions required before a second engine can be trusted:**

1. A new engine is a **new untrusted client by default**. It must authenticate as a normal user and hold only the anon key + user JWT.
2. **No service_role, no Postgres password, no direct table writes** in any non-Unity project. Enforce with a repo-level secret scan (see ME-2).
3. A client identifier must **not** become an authorisation input. `GameCommand` carries `playerId?`; the server always uses `auth.uid()` — keep it that way, and drop `playerId` from any new client-facing schema.
4. Mock mode must be **structurally impossible to confuse with live data**: a `source: "MOCK"` field, persistent UI banner, and no shared code path that can silently point at a real URL.
5. A second engine must never be given a disposable-account test mutation path against production; see §11 ME-4.

---

## 7. Data flow

### 7.1 Read (already supported, engine-neutral)

```
client ── JWT ──► POST /rest/v1/rpc/naad_get_wallet {}          → { …, balance }
                  POST /rest/v1/rpc/naad_get_world_clock {}     → clock/period/weather
                  POST /rest/v1/rpc/naad_list_messages {}       → message rows
                  POST /rest/v1/rpc/naad_get_relationship { p_target_id }
```

### 7.2 Command (already supported, engine-neutral)

```
client ── JWT ──► POST /rest/v1/rpc/naad_execute_command
                  { p_request_id: "<uuid>", p_type: "SPEND", p_payload: { "sku": "SUYA_PLATE" } }
        ◄── { requestId, success, errorCode, errorMessage, payload }
        replay with the same p_request_id ◄── identical receipt (verified in naad_travel logic; the same pattern is assumed for other handlers [ASSUMPTION])
```

### 7.3 Cross-engine demo target (ME-4)

A Godot client calls exactly the two shapes above. No new endpoint.

---

## 8. Proposed contract versioning (proposal for ME-2 — not implemented)

**[RECOMMENDATION]**

- **Directory:** `contracts/naad/v1/` — a new top-level `contracts/` because the current candidates (`world-model/schemas`, `backend/game-api/src/contracts`) are respectively *entity shapes* and *Unity-facing TS*; neither is a cross-engine wire contract. If ME-2 review prefers depth over breadth, `backend/game-api/src/contracts/v1/` is a defensible alternative that avoids a new top-level folder.
- **Format:** JSON Schema Draft 2020-12, matching `world-model/schemas` exactly. No new dialect.
- **Version line:** the major version is in the directory (`v1`); every root schema carries `"$id": "https://naad.game/contracts/v1/<name>.schema.json"` and a `x-naad-contract-version: "1.0.0"` annotation. Additive optional fields → minor bump; any removal or type change → `v2` directory.
- **Start minimal — four files, not nine.** Only what a real cross-engine demo needs:
  1. `world-command-request.schema.json` — `{ requestId (uuid, required), type (enum of the 14, required), payload (object) }`
  2. `world-command-result.schema.json` — `{ requestId, success, errorCode (enum \| null), errorMessage, payload }`
  3. `error-code.schema.json` — closed enum: `UNAUTHENTICATED`, `INVALID_REQUEST`, `LOCATION_NOT_FOUND`, `ALREADY_THERE`, `UNSUPPORTED_COMMAND`, `COMMAND_EXCEPTION`, `INSUFFICIENT_FUNDS` — **[must be confirmed against every SQL handler in ME-2 before freezing]**
  4. `player-wallet-view.schema.json` — `{ balance, currency: "NGN", updatedAt }` (server-decided; no client balance in any request)
  - Deferred: `player-profile`, `player-inventory`, `relationship-summary`, `game-session`, `world-geographic-location`, `world-event` — **the prompt already warns against creating every candidate**, and four of them (`inventory`, `game-session`, `npc-memory`-adjacent) are marked "not live in Postgres" in `world-model/README.md`.
- **Reuse, don't rename:** where a contract maps to an existing `world-model/schemas` entity, `"$ref"` it (e.g. wallet view → `wallet.schema.json` shape minus `playerId`).
- **Stable identifiers:** UUID strings; location IDs from `LOCATION_IDS`/seed suffix convention; `requestId` a client-generated UUID-v4 string, never reused except for an intentional replay.
- **Timestamps:** RFC 3339 / `format: "date-time"`; money integer NGN as in `wallet.schema.json`.
- **Fixtures:** `contracts/naad/v1/fixtures/{valid,invalid}/*.json`, one pair per schema.
- **Validation test:** zero-dependency, in the style of `contracts.test.ts`/`validate.test.ts`, runnable as `node --experimental-strip-types contracts/naad/v1/validate.test.ts`, with a **minimal in-repo JSON-Schema subset validator** (the keywords actually used: `type`, `required`, `enum`, `const`, `properties`, `additionalProperties`, `format: uuid|date-time`, `minimum`, `maximum`, `minLength`) so that no dependency needs installing to start. **[RECOMMENDATION]** If a real validator (`ajv`) is desired instead, that is a dependency decision for approval, not an assumption.
- **Anti-drift tests (the part that makes this worth doing):** a test that parses `0009_execute_command_router.sql`'s `when '<TYPE>'` clauses and asserts set equality with `COMMAND_REGISTRY` and with the schema enum; and a test that asserts every error code emitted by the SQL files appears in `error-code.schema.json`.
- **Secrets:** none of the four schemas may contain a key, JWT, project URL or balance-supplying request field.

---

## 9. Godot prototype scope (proposal for ME-3 — not started)

- **Location:** `game/godot/nightlife-mini-game/` (reuse the `game/` parent; see §2.6). Open question for review: `game/godot/…` vs `experiences/godot/…`.
- **Content:** one room, placeholder capsule/sprites, a walkable area, one interactable (a bar counter or one NPC), one action button, a result panel.
- **Data:** a **local fixture adapter** implementing the same four ME-2 shapes, returning canned results, with a permanent `MOCK` badge. No HTTP in ME-3.
- **Explicitly out of scope:** accounts, wallet, inventory, persistence, a second world, real art packs, any paid asset, network calls.
- **README must state:** required Godot version (pin an exact one when created — do not write "latest"), how to open/run, how to run its tests, mock behaviour, which ME-2 schema files it reads, and known limitations.
- **Isolation rule:** no file outside `game/godot/…` is edited to make it work; Unity `Packages/`, `ProjectSettings/`, and `Assets/` are untouched.

---

## 10. Risks and mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|
| R1 | A second client re-implements economy rules (as `MockCommandService` already does) | High | Critical | ME-2 adds an explicit rule: clients *never* compute `balanceAfter`, `INSUFFICIENT_FUNDS`, or prices. Mock adapter returns canned server-shaped results only |
| R2 | Contract drift across SQL / TS / C# / new engine | High | High | Anti-drift tests (§8); CI job that runs them |
| R3 | Hand-committing the 1,376-entry dirty tree | Medium | High | Per-file staging only; dedicated branch from `93d076b`; never `git add -A` |
| R4 | Unity cannot be verified, so "Unity unchanged" is asserted, not proven | High | Medium | The claim in this gate is *scope*: zero Unity files were edited. **[RECOMMENDATION]** Capture `git diff --stat -- game/` before/after ME-2 as evidence |
| R5 | Destructive cleanup to "fix" Unity | Low | Critical | Forbidden; the LLMUnity deletion is user work, recoverable via git |
| R6 | Godot work becomes a second product | Medium | High | Hard scope cap in §9; ME-5 gate decides keep/defer/rework/stop |
| R7 | Mock results read as real transactions | Medium | High | `source: MOCK` in every fixture, UI banner, no live URL compiled in |
| R8 | Production mutation from a test client | Low | Critical | No staging → stop at mock boundary (§11 ME-4); no production credentials in any new project |
| R9 | New top-level `contracts/` and `experiences/` folders add repo sprawl | Medium | Low | Prefer existing parents (`backend/game-api/src/contracts/v1`, `game/godot/`) |
| R10 | Unity version premise in the prompt is wrong for this checkout | — | Medium | §1.1; the committed version is `6000.7.0b4`, the working tree has an uncommitted `6000.6.5f1` downgrade. Never restate a version without `git diff -- ProjectVersion.txt` |

---

## 11. Gate acceptance criteria

### ME-1 (this gate)
- [x] Repository, branch, commit, working tree and local modifications recorded.
- [x] Actual command path traced (`0009` router → handlers → receipts → events), not inferred from filenames.
- [x] Existing contracts/schemas inventoried, including the three-copy drift problem.
- [x] Security boundary re-verified.
- [x] Two tests executed with real results recorded (§1.3).
- [x] No Unity, migration, backend, contract, lockfile or deploy change.
- [ ] **Not claimable:** Unity compiles. (No evidence; blocked externally.)

### ME-2 entry conditions (all must hold)
1. Branch `feat/me2-engine-neutral-contracts` created from `93d076b`; working tree clean for the paths being touched.
2. Write scope limited to `contracts/naad/v1/**` (or the approved alternative) + one line in `docs/`.
3. Error-code enum confirmed by reading every `errorCode` literal in `0003`–`0009`, not from memory.
4. `node --experimental-strip-types contracts/naad/v1/validate.test.ts` prints `ok` and exits 0; the anti-drift test parses the SQL and passes.
5. `contracts.test.ts` and `validate.test.ts` still pass after the change.
6. No new npm dependency added without explicit approval.
7. `git diff --stat -- game/ supabase/` is empty.

### ME-3 entry: ME-2 checklist complete **and** Godot version pinned.
### ME-4 entry: ME-3 runs **and** a staging Supabase project + disposable account exist. If not → stop, document.
### ME-5 entry: ME-4 findings recorded (or an explicit "stopped at mock boundary" record).

---

## 12. Changes made by this gate

| File | Change |
|---|---|
| `docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md` | **Added** — this document |

Nothing else. Verified scope: no file under `game/`, `supabase/`, `backend/`, `world-model/`, `ai/`, `tools/`, `.github/` was created, edited or deleted by this gate.

**Recommended commit** (on a branch cut from `93d076b`, staging only this file):

```
git switch -c docs/me1-multi-engine-architecture 93d076b
git add docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md
git commit -m "docs(me1): multi-engine architecture audit and decision record"
```

Do **not** `git add -A`: it would stage ~200 LLMUnity deletions plus ~60 unrelated `.fbx.meta` files.

---

## 13. Explicitly deferred

- ME-2 contract schemas, fixtures, validator and anti-drift tests.
- Any Godot project, Godot install, or Godot asset.
- Any Unity code, scene, package, `ProjectSettings` or version change.
- Any migration, RPC, Edge Function or RLS change.
- Any new dependency (`ajv`, `supabase-js`, Godot export templates).
- CI changes and the stale Unity pin / version mismatch in §1.1.
- Fixing the untracked-but-required C# file problem (§2.8 item 3) and the `MockCommandService` economy duplication (R1) — both are their own focused changes.
- PR #11 status, `main`, deployment, and the live First Night test.

---

## 14. Remaining blockers and next gate

**Blockers that do not stop ME-2 (it is pure file work, no Unity, no network):** none of the §2.8 items block it.

**Blockers that stop ME-3/ME-4:** Unity verification (irrelevant to Godot but relevant to any "Unity still fine" claim), and the absence of a staging Supabase project + disposable account for ME-4.

**Next gate: ME-2**, with the seven entry conditions in §11 and the scope limits in §8. Return the schema files, the fixture set, the test output, and `git diff --stat` before ME-3 is authorised.
