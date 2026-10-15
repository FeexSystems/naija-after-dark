# NAAD — Full Project Plan

**Date:** 2026-10-14 · **Branch:** `feat/audit-gate-recovery-2026-10-14`  
**Editor:** `6000.6.5f1` · **Live backend:** Supabase project `unzfqrfyejkyisalzkhc`

## 1. Vision (one paragraph)

NAIJA AFTER DARK is a Nigerian-nightlife life-simulation **vertical slice**: a
server-authoritative world (Postgres/Supabase is the source of truth) that Unity
renders as an untrusted thin client, with Gemini as a proposals-only AI layer.
The slice proves one believable night — arrive, meet people, build trust, spend,
travel, shape reputation — before any city-scale expansion. Discipline over
scope: the world model is authoritative, the client never mutates it directly,
and AI never overrides it.

## 2. Architecture pillars (non-negotiable)

1. **World-state authority** — Postgres is truth. Client sends *commands*; server
   validates + applies. No client-side wallet/relationship/location truth.
2. **Single command surface** — all mutations go through `naad_execute_command`
   (type + payload + `request_id`); reads stay separate. Idempotency on
   `command_receipts.request_id`.
3. **AI with boundaries** — Gemini interprets/proposes dialogue only; it never
   writes the DB. Server validates the response against a JSON schema.
4. **Ephemeral ≠ persistent** — rooms/presence are hints; money/relationships/
   location are durable.
5. **Presentation vs logic separation** — animation/audio/VFX react to state;
   they never trigger authoritative changes (the Gate C invariant).

## 3. Trust model / module map

```
Unity (untrusted client)
  ├── Auth                (Supabase auth, session restore)
  ├── Commands            → POST /rest/v1/rpc/naad_execute_command
  ├── Reads               → get_wallet, get_clock, list_messages, list_events, presence
  └── AI talk             → Edge Function npc-dialogue (JWT, Gemini)

Supabase (authoritative)

## 4. Gate roadmap (current truth, 2026-10-14)

Legend: ✅ Verified · 🟡 Implemented/pending CI · ⬜ Planned

### Backend / world model (server-authoritative)
| Gate | Scope | Status |
|---|---|---|
| 7 | First World — Lagos Metro → Night District (Apartment, Street, Suya, Nightclub, Beach) | ✅ Verified on live DB |
| 8 | NPCs — Tunde (AI), Mama Seyi, Amaka, Emeka, Dami (deterministic) | ✅ Verified |
| 9 | Gemini dialogue path (Edge Function, key server-side, schema-validated) | 🟡 code + schema; Edge deploy + key pending |
| 10 | NPC memory (memoryCandidate → commit, dedup 24h, top-5 context) | ✅ Verified |
| 11 | Economy — SKUs + `naad_get_wallet` / `naad_spend` | ✅ Verified |
| 12 | Relationships — `naad_apply_relationship_delta` (±10 clamp, 0–100) | ✅ Verified |
| 13 | Phone — Tunde beach invite (GO/ASK/DECLINE) + `naad_respond_message` | ✅ Verified |
| 14 | First Night server loop — start → phone → spend/travel → complete → summary | ✅ Verified |
| 15 | Multiplayer rooms — `game_rooms` + `room_presence` (ephemeral) | ✅ Verified |
| 17 | Content expansion — more locations/careers + `naad_set_career` | ⬜ Planned |
| 18 | Social graph — `opportunities` + refresh/accept | ✅ Verified |
| 19 | Events — `world_events` + `event_attendance` (Beach Party, Club Night) | ✅ Verified |
| 20 | World reactivity — attendance raises nightlife/traffic; `WORLD_REACTIVITY` event | ✅ Verified |

### Client / integration (Unity)
| Gate | Scope | Status |
|---|---|---|
| 21 | Unity integration — reproducible, compiles, `NAADBuild.Verify` green | 🟡 local green (6 scenes); CI pending secrets |
| 22 | Reproducible Unity CI baseline — editor version match + licensing preflight | 🟡 workflow wired; first green run pending |
| B | First Night vertical slice live end-to-end (Gate B / Phase D) | 🟡 verified locally; CI pending |
| C | Player rig & animation — see `GATE_C_PLAYER_RIG_ANIMATION.md` | ⬜ Planned (next gate) |
| D | Gameplay/interaction — third-person controller + interaction framework | ⬜ Planned |
| E | Atmosphere/simulation — day/night lighting, crowd, world clock | ⬜ Planned |

## 5. Sequenced execution roadmap (recommended order)

**Now → unblock the first green CI (days, not weeks):**
1. Repository Actions secrets set (`UNITY_EMAIL`, `UNITY_PASSWORD`,
   `NAAD_TEST_EMAIL`, `NAAD_TEST_PASSWORD`) — **done per screenshot**.
2. Open the PR for `feat/audit-gate-recovery-2026-10-14` → triggers CI
   (`verify-unity`, `verify-project-structure`, `list-packages`,
   `first-night-live-test`). Record run URL + SHA in `GATE_STATUS.md`.

**Gate C — player rig & animation** (next client gate):
- Follow `GATE_C_PLAYER_RIG_ANIMATION.md` (C1 acquire rig → C2 clips → C3
  controller+mask → C4 prefab → C5 verify + CI). Free asset path (Mixamo).

**Engineering Phase C — AI dialogue live:**
- `supabase secrets set GEMINI_API_KEY`, deploy `npc-dialogue`, wire Unity
  `TalkToTunde`. Gemini still never writes DB.

**Engineering Phase D — contract hardening:**
- DTO structs (JsonUtility) over string search; SQL/TS contract tests (invalid
  spend, double request_id, same-location travel); RPC catalog in
  `backend/game-api`; structured `requestId` logging.

**Gate D — gameplay/interaction:**
- Third-person character controller (script-driven, per Gate C decision) + an
- interaction framework over the scene `SceneAnchor`s already validated in
  scenes (spawn/exit/interact). Connect interactions to authoritative commands.

**Gate E — atmosphere:**
- Day/night lighting profiles, `WorldClockPresenter` integration, crowd/NPC
  presence visuals (hints only — server owns counts).

**Gate F + Gate 16 — mobile:**
- Phone apps UI, then **profile on device first** (`GATE16_MOBILE_OPT.md`):
  FPS/RAM/CPU/GPU/battery/size/load on low/mid/high Android + iPhone, only then
  optimize. Android device acceptance + on-device First Night → Summary.

**Later / optional:**
- Engineering Phase E (Presence Realtime), Gate 17 (content expansion).

## 6. Explicit non-goals (guardrails)

- No microservices split. No Unity networking rewrite. No Redis/Kafka/custom
  game server. No full-city expansion yet. No art/CPU optimization before Gate 16
  measurements. No service-role or Gemini keys in Unity. No client-side
  authoritative state. Animation events never mutate game state.

## 7. Risks & mitigations

| Risk | Mitigation |
|---|---|
| CI licensing / secrets misconfiguration | Preflight fails fast with a clear message; secrets are repo-level now |
| Unity lock collisions in local batch runs | Kill lingering Unity + wait before each run (proven in this audit) |
| Animator param-name drift (Gate C silent no-op) | `VerifyAnimationAssets` contract assertion (param names match code) |
| Drift between TS contracts and SQL RPCs | Contract tests + RPC catalog (Phase D) |
| Client trusting itself for wallet/state | Single command surface + server-side idempotency/validation |
| Premature optimization | Gate 16 profile-on-device gate before any perf work |

## 8. Success metrics ("done enough")

1. One Unity build completes First Night against the live project, **in CI** (green run recorded).
2. All mutations go through ≤ 2 entry points (command RPC + AI edge).
3. Contract list in `backend/game-api` matches live RPCs.
4. Zero service-role usage in the client.
5. Domain events queryable for a single night session.
6. Gate C animator contract verified headless; player moves + phones + greets.
7. First Night reaches Night Summary **on a physical device**.

## 9. Related docs

- `GATE_C_PLAYER_RIG_ANIMATION.md` — next gate detail.
- `NEXT_ARCHITECTURE.md` — engineering phases A–F.
- `GATES_7_10.md`, `GATES_11_14.md`, `GATES_15_20.md`, `GATE_21.md`, `GATE_22.md` — backend + CI gate specs.
- `GATE_STATUS.md` — live status ledger. `GATE16_MOBILE_OPT.md` — mobile profiling gate.
- `ARCHITECTURE.md`, `WORLD-MODEL-SPEC.md` — system + world model.

| F | Mobile UI / AI / release — phone apps, Android device acceptance | ⬜ Planned |
| 16 | Mobile optimization — profile-on-device before optimizing | ⬜ Planned (after on-device play) |

> Note: engineering "Phase C" (AI dialogue) is distinct from client "Gate C"
> (player rig/animation). This plan uses **Gate C = player rig/animation**.

  ├── Postgres world model + RPCs + RLS
  ├── Auth
  ├── Realtime (later: presence)
  └── Edge Functions (Gemini only; keys server-side)

ai-orchestrator
  └── validate + prompt assembly (shared with Edge Function)
```

No new deployable service until a measured bottleneck appears (explicit non-goal).
