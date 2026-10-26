# NAAD — Sophisticated Creative Scenes Implementation Plan

**Status:** PLAN ONLY. No code, scene, migration, contract or asset was created or modified by this document.
**Date:** 2026-10-25 (Africa/Lagos)
**Companion to:** `docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md` (ME-1 audit)
**Rule:** one `requestId` in, one validated result out. No scene mutates state directly (§2).

Tagging convention: **[VERIFIED]** = observed in this checkout; **[ASSUMPTION]** = inferred, unproven; **[RECOMMENDATION]** = proposed, not implemented.

---

## 1. The uncomfortable truth this plan is built around

The repo already has more *simulated systems* than it has *rendered scenes*. Increasing the scene count without fixing that ships prettier emptiness.

**[VERIFIED] What exists server-side today** (25 tables across `0001`–`0009`):

`businesses, career_defs, command_receipts, districts, domain_events, event_attendance, game_rooms, items, locations, messages, night_sessions, npc_dialogue_lines, npc_memories, npcs, opportunities, player_careers, player_locations, players, price_list, relationships, room_presence, wallet_transactions, wallets, world_events, world_state`

**[VERIFIED] What exists as rendered scenes:** 6 registered build scenes — `Bootstrap`, `Apartment`, `CityStreet`, `SuyaSpot`, `Nightclub`, `Beach` (`docs/UNITY_SCENE_REGISTRY.md`, `game/unity/Assets/NAAD/Scenes/*.unity`).

**[VERIFIED] The gap between them:**

| Server system | Client consumption | Scene consequence |
|---|---|---|
| `game_rooms`, `room_presence`, `naad_update_presence`, `naad_list_room_presence` | **zero calls** from any Unity `.cs` | Nightclub is a room with no other people in it |
| `world_events`, `event_attendance`, `naad_list_events` | **zero calls** | The two seeded events (`Beach Party`, `Club Night`) are invisible |
| `opportunities`, `naad_refresh_opportunities` | **zero calls** | Trust with Tunde opens nothing the player can see |
| `career_defs`, `player_careers`, `naad_set_career` | only via generic command path | Careers are a DB row, not a place |
| `npc_dialogue_lines`, `npc_memories` | greeting + commit only | NPCs can remember, but cannot react to a room |
| `weather` on `world_state` | clock read returns it | No weather presentation in any scene |
| `businesses` | **zero calls** | Venues have no owners, prices, or staffing |
| `naad_advance_world_time` | called | the only wired "world reacts" signal |

**[VERIFIED] There is no `reputations` table.** `0008` adds `required_reputation` to `world_events`, but nothing computes or stores reputation. `world-model/schemas/reputation.schema.json` is a schema without a home. This is the single biggest blocker to "sophisticated" scenes: gating scenes on reputation is impossible until reputation exists.

**[VERIFIED] Scene transitions do not exist.** `NAADBootstrap.nextSceneName` is empty; travel is *authoritative server travel*, not scene loading. Only `Bootstrap` and (by internal load) the first-night presentation run. So "scene" today means *a registered scene file*, not *a place the player can walk into*.

**Implication for any plan here:** sophisticated scenes are **90% server + wiring + 10% art**. A scene that is only art cannot be validated by any existing gate (`NAADBuild.Verify`, `ValidateScenes`, `contracts.test.ts`) and therefore cannot be claimed done. That constraint shapes every option in §4.

---

## 2. The eight rules every proposed scene must obey

Non-negotiable, derived from `docs/NAAD_MULTI_ENGINE_ARCHITECTURE.md` §4–§6 and the existing migrations:

1. **Clients hold no decisions.** A scene may *request* and *display*; it never computes money, prices, relationship deltas, or eligibility.
2. **No new mutation RPC.** Scenes use `naad_execute_command` with one of the existing 14 types, or a read RPC that already exists. Any new command type is its own approved migration, not part of a scene PR.
3. **Idempotency is structural.** Every scene action carries a stable `p_request_id` derived from `(playerId, sceneId, actionKey)`; replaying a tap must return the receipt, not double-charge.
4. **AI is a performer, not a referee.** Dialogue and NPC lines come from `npc-dialogue` / `validateDialogueResponse`; any *effect* the AI implies is re-validated server-side (existing `COMMIT_MEMORY` / `RELATIONSHIP_DELTA` pattern).
5. **Ephemeral ≠ persistent.** Presence, crowd, and pose are hints (`room_presence`); wallet, inventory, relationships, progression are authoritative. A scene must never promote a presence hint into a decision.
6. **Every scene action must be replayable headless.** If it cannot be exercised by an editor `-executeMethod` entry point with the stub/mock services, it cannot be verified in this environment (Unity cannot complete AssetDatabase startup — `NAAD_BASELINE_AUDIT.md`), so it cannot be claimed done.
7. **Mock must be structurally distinct from live.** `source: "MOCK"` in payloads, persistent banner, no live URL compiled into a mock path.
8. **No scene may require reputation until reputation exists.** Gating on a non-existent system is how scenes get built that can never open.

---

## 3. What "sophisticated" should mean here

Not "more polygons". For NAAD, sophistication is **the scene reading and writing the parts of the World Model the player cannot see**, in a way that other players can also feel. Ranked by that definition:

| Tier | Meaning | Example |
|---|---|---|
| T0 | Art only, no state | a nicer nightclub interior |
| T1 | Scene reads authoritative state | club interior that looks different at `PACKED` vs `LOW` |
| T2 | Scene proposes a validated command | bar order → `SPEND` → server-decided balance |
| T3 | Scene reflects *other players* | presence avatars in the club from `room_presence` |
| T4 | Scene changes the world for others | your attendance raises `event_attendance`, which drives `naad_react_to_event_attendance` |
| T5 | Scene remembers the player | NPC memory surfaces as different dialogue next visit |

**Only T2–T5 are worth gating.** T0/T1 scenes are art tasks with a scene name.

---

## 4. Candidate scenes — ten proposals, ranked

Each entry states what makes it sophisticated, what already exists, what is missing, and what it costs.

### SC-1 · The Room That Breathes — occupied Nightclub (T3/T4)
**Pitch:** the club you already have, but populated: figures move, the crowd reacts to the event's `crowd_profile` (`LOW/MEDIUM/HIGH/PACKED`), and your own presence is visible to others.

- **Reuses:** `game_rooms`, `room_presence`, `naad_join_room` / `naad_leave_room` commands, `naad_update_presence` (pose hint), `naad_list_room_presence`, `world_events.crowd_profile`.
- **Missing:** any Unity call to four of those RPCs **[VERIFIED: zero callers]**; a presence poll or Realtime subscription (`docs/NEXT_ARCHITECTURE.md` Phase E, still open); NPC crowd visuals without paid assets.
- **Why sophisticated:** it makes the existing `PACKED` enum *visible* and makes other players exist. Highest ratio of "feels alive" to "new truth created" — it creates none.
- **Cost:** M. **Risk:** presence used as authority (rule 5) — must stay a hint.
- **Verifiability:** headless test can assert join → presence row → leave → row gone, with no rendering.
- **Verdict:** **[RECOMMENDATION] Build first.** It is the largest payoff for the least new state, and it is the only candidate that needs **no migration**.

### SC-2 · The Suya Queue — a place with a rhythm (T2/T3)
**Pitch:** SuyaSpot is not a vending machine. A queue of NPCs and players forms, Mama Seyi works through it, and ordering is a timed social act rather than a button.

- **Reuses:** `SPEND`/`SUYA_PLATE` (5,000 NGN, in `price_list`), `naad_spend`, `room_presence` for the queue, `npcs` for Mama Seyi.
- **Missing:** no queue concept anywhere; **no RPC exposes the queue**; queue position would be new state — and queue position *is* gameplay-relevant, so it cannot be presence-only.
- **Why sophisticated:** scarcity and waiting are the cheapest strong feelings in a nightlife game; a queue turns a transaction into a scene.
- **Cost:** M/L. **Risk:** queue state becomes a second source of truth (rule 1/5). **[RECOMMENDATION]** model the queue as *derived* from `room_presence` ordered by `joined_at`, never as its own table.
- **Verifiability:** headless: two stub players join, queue order is deterministic, spend still validated by server.

### SC-3 · The DJ Booth — AI as performer under supervision (T5)
**Pitch:** Tunde on the decks. The set list and crowd calls are AI *proposals*, the room reacts, and what Tunde remembers about you colours the next set.

- **Reuses:** `npc-dialogue` Edge Function, `validateDialogueResponse`, `naad_npc_dialogue_context`, `naad_commit_npc_memory`, `npc_memories`, `naad_get_npc_greeting`.
- **Missing:** a performance-shaped prompt and output schema (`ai/schemas/npc-dialogue-response.schema.json` is conversation-shaped, not set-shaped) **[ASSUMPTION — schema not re-read for this plan]**; cost controls; cached/offline fallback so a venue does not go silent when the API does.
- **Why sophisticated:** the AI is the live act, and memory makes it continuous across nights. This is the most "NAAD" idea in the list.
- **Cost:** M. **Risk:** latency and cost per player; AI implying economic effects (rule 4). **Mitigation:** proposal-only, deterministic fallback lines from `npc_dialogue_lines`.
- **Verifiability:** validate-at-the-boundary test already exists in spirit (`validate.test.ts`); add a fixture set of malformed model outputs.

### SC-4 · The Opportunity — the world notices you (T4)
**Pitch:** you talked to Tunde enough; something opens. A DJ intro, a gig, event access, a favour. The existing `opportunities` table already models exactly this and is **unused by the client**.

- **Reuses:** `opportunities`, `naad_refresh_opportunities`, `ACCEPT_OPPORTUNITY` command, `relationships.trust`.
- **Missing:** zero client calls **[VERIFIED]**; no notification surface; **no reputation system** to gate anything broader.
- **Why sophisticated:** it is the first system where *the world initiates*. That is the difference between a sandbox and a story.
- **Cost:** S/M — genuinely the smallest server-side lift in this list, because the table and trigger already exist.
- **Verifiability:** headless: raise trust → refresh → opportunity row appears → accept → status `ACCEPTED` → no double-accept.

### SC-5 · Rain on the Street — weather that changes behaviour (T1/T2)
**Pitch:** `world_state.weather` (`CLEAR/CLOUDY/RAIN/HEAVY_RAIN/HARMATTAN`) drives the street scene: puddles, fewer pedestrians, okada prices.

- **Reuses:** weather already returned by `naad_get_world_clock`.
- **Missing:** any visual weather layer; pedestrian density system; **price variation by weather is NOT supported** — `price_list` has no weather dimension **[VERIFIED: columns are `sku, name, category, price_ngn, location_type`]**.
- **Why sophisticated:** cheap, high-atmosphere, and it proves the world clock is real. Harmattan in Lagos is instantly recognisable.
- **Cost:** S (visuals) / M (if prices must vary — that is a migration and an economy change; **defer the price part**).
- **Verdict:** **[RECOMMENDATION] Best "quick win" creative scene.** Ship visuals only; refuse the price link until the economy owner approves it.

### SC-6 · The Beach at After Hours — time as a place (T1/T4)
**Pitch:** the Beach scene changes identity by `world_state` period: sunset, night market, after-hours quiet. The seeded `Beach Party` event becomes visible and attendance matters.

- **Reuses:** `world_events` (seeded `Beach Party`), `ATTEND_EVENT` command, `event_attendance`, `naad_react_to_event_attendance`, period system in `0004`.
- **Missing:** `naad_list_events` has zero callers **[VERIFIED]**; no "event in progress" visual state; `start_time`/`end_time` are seeded with `now() + interval`, so **events expire permanently after seeding** — a real design flaw **[VERIFIED: `330–348`]**.
- **Why sophisticated:** a place that changes with time is the cheapest form of living world. It also forces the event-expiry bug to be fixed properly.
- **Cost:** M. **Risk:** time-gated content is unverifiable in CI unless the clock is injectable — currently `now()` is read inside SQL.
- **Verifiability:** headless only with a clock seam **[RECOMMENDATION] add an `as_of` parameter to read RPCs, not to commands.**

### SC-7 · The Apartment Aftermath — memory as décor (T5)
**Pitch:** the apartment the morning after reflects what actually happened: items bought, messages received, people you met.

- **Reuses:** `messages`, `wallet_transactions`, `npc_memories`, `night_sessions`.
- **Missing:** an inventory read surface — `inventories` **does not exist as a table** (`world-model/README.md` lists it "no") **[VERIFIED]**; so "things you bought" can only be inferred from `wallet_transactions`, which is a ledger, not possessions.
- **Why sophisticated:** continuity is what makes a life-sim feel lived-in rather than reset.
- **Cost:** S/M if scoped to messages + transactions; **blocked** if it needs inventory.
- **Verdict:** **[RECOMMENDATION] Scope to messages and receipts only.** Do not invent inventory for a scene.

### SC-8 · The Restaurant Table — a conversation with a cost (T2/T5)
**Pitch:** `0008` seeds a `Restaurant` location (`…1106`) that has **no scene, no price entries, and no NPC**. A table where an NPC joins you: every topic costs something and relationship deltas follow.

- **Reuses:** `RELATIONSHIP_DELTA`, `naad_apply_relationship_delta`, `npc-dialogue`, `opportunities`.
- **Missing:** a Restaurant scene; price rows for it; an NPC assigned there; the "who is at my table" concept.
- **Why sophisticated:** relationships are NAAD's real currency; right now they only move through hidden commands.
- **Cost:** M. **Risk:** it is the most content-hungry option (dialogue authoring), so it is the most likely to stall.
- **Verdict:** defer behind SC-1 and SC-4.

### SC-9 · The Careless Circuit — risk with real teeth (T2/T4)
**Pitch:** a late-night street scene where a bad choice costs money and reputation. Careers (`career_defs`) and gigs (`opportunities` type `GIG`) become the recovery path.

- **Reuses:** `SPEND`/`RELATIONSHIP_DELTA`/`SET_CAREER`, `career_defs`, `opportunities`.
- **Missing:** **reputation does not exist** [VERIFIED]; no failure-state modelling; no consequence system beyond `INSUFFICIENT_FUNDS`.
- **Why sophisticated:** consequence without punishment is the hardest and most valuable thing in a nightlife sim.
- **Cost:** L. **Risk:** **blocked by the missing reputation system.** Do not start.
- **Verdict:** **[RECOMMENDATION] Blocked.** Listed so it is not quietly attempted later.

### SC-10 · The Cross-Engine Window — one room, two renders (multi-engine proof)
**Pitch:** the Godot mini-game (ME-3) and Unity both render the **same** `Nightclub` room state from the same four ME-2 contracts, side by side.

- **Reuses:** ME-2 contracts, `naad_get_world_clock`, `naad_list_room_presence`, `naad_execute_command`.
- **Missing:** ME-2 (not started), ME-3 (not started), staging project for ME-4.
- **Why sophisticated:** it is the only way to *prove* the multi-engine thesis rather than assert it.
- **Cost:** S *after* ME-2/ME-3, impossible before.
- **Verdict:** the real ME-4 demo target. Not a first scene.

---

## 5. Recommended sequence

Ordered by (payoff ÷ new truth created) and by what can actually be verified in this environment.

```text
Phase 0  Unblock verification            (prerequisites, no scenes)
   │
Phase 1  SC-1 occupied club      ──►  SC-4 opportunities
   │        (no migration)              (no migration)
   │
Phase 2  SC-5 weather on Street  ──►  SC-6 Beach by period + events
   │        (visual only)                (fixes event expiry)
   │
Phase 3  SC-7 apartment aftermath ──► SC-2 suya queue
   │        (messages + receipts)        (derived from presence)
   │
Phase 4  SC-3 DJ booth (AI performer)
   │
Phase 5  SC-8 restaurant table  ──►  SC-10 cross-engine window (with ME-4)
   │
Blocked  SC-9 careless circuit  (needs reputation — do not start)
```

**Why SC-1 and SC-4 first:** both use tables that already exist, need **no migration**, and are consumed by **zero client code** today. They convert *existing dead server systems* into *live scenes* — the highest-leverage work in the repo — and they are fully headless-testable.

**Why not the flashiest scene first:** SC-3 (AI DJ) and SC-9 (risk) are the most exciting and the most expensive/blocked. Leading with them would mean building the biggest scene on the least verified foundation — exactly the failure mode the ME-1 audit warned about.

---

## 6. Phase 0 — prerequisites (before any scene)

| # | Item | Why it gates scenes | Evidence that it is done |
|---|---|
| P1 | **Scene transition exists.** Wire `NAADBootstrap.nextSceneName` → registered scene, or define an explicit in-scene "portal" that a validated `TRAVEL` result authorises | Without it a "scene" is a file nobody can enter (`UNITY_SCENE_REGISTRY.md` §Bootstrap transition note) | headless: travel command success → transition requested for the registered scene name; failure → no transition |
| P2 | **Unity runs headless at all.** Resolve the AssetDatabase startup hang or establish one working machine/CI image | No scene can be visually or headless verified today (`NAAD_BASELINE_AUDIT.md`) | one recorded green `NAAD.Editor.NAADBuild.Verify` |
| P3 | **Reputation decision.** Either add a `reputations` table (migration) or formally delete reputational gating from `world_events` | Every "sophisticated" gate currently references a column with no source | recorded decision in `docs/` |
| P4 | **Event lifecycle fix.** Seeded events use `now() + interval` and go stale forever (`0008` 330–348) | SC-6 cannot demonstrate anything if events are permanently ended | read test: `naad_list_events` returns rows after a simulated day |
| P5 | **Clock seam for reads.** `as_of timestamptz default now()` on read RPCs | time-of-day scenes are untestable otherwise | headless: same fixture, two `as_of` values, two different period outputs |
| P6 | **Index/fixture for scenes.** A `sceneId` constant set shared with contracts | prevents scene IDs diverging like the command vocabulary did (3 copies) | anti-drift test |

**[RECOMMENDATION]** P4 and P5 are small, server-side, and unblock three of the ten scenes. Do them before Phase 1 if time allows; otherwise Phase 1 (SC-1, SC-4) proceeds without them.

---

## 7. Per-scene specification format (what each implementation PR must contain)

Every scene PR — one scene, no bundling:

1. **Scene ID** (stable, added to P6 registry).
2. **Reads:** exact RPCs, exact fields, and the empty-state behaviour.
3. **Writes:** exact `naad_execute_command` type + payload; `request_ids` derivation rule.
4. **State ownership table:** which fields are authoritative, which are hints (rule 5).
5. **Failure choreography:** what the player sees for `INSUFFICIENT_FUNDS`, `ALREADY_THERE`, `UNAUTHENTICATED`, network loss, AI timeout. (A scene that only has a happy path is not sophisticated, it is fragile.)
6. **Mock mirror:** fixture adapter returning ME-2-shaped canned results with `source: "MOCK"`.
7. **Headless test:** the exact `-executeMethod` entry point and assertions.
8. **Screenshot:** one frame, taken and *looked at*, not described.
9. **Explicit non-goals.**
10. **Cost/performance note** for Android (target platform is Android-first).

---

## 8. What would make these scenes genuinely sophisticated (design notes)

Not features — *qualities* that separate a memorable scene from a decorated room:

- **Latency as texture.** A queue that takes time is better than an instant purchase. Use server round-trip time deliberately rather than hiding it.
- **Silence.** NPCs that sometimes do not greet you because the memory says they are upset. `npc_memories` supports this; nothing uses it.
- **Second-order consequences.** Buying the club ticket should be visible in the Apartment aftermath scene. Scenes should echo each other through `domain_events`, not through hard-coded links.
- **Weather and time as a shared language.** If all scenes read the same clock (`0004`), the world feels singular.
- **Failure as content.** `INSUFFICIENT_FUNDS` at the bar is a *scene* (Mama Seyi's reaction), not a toast.
- **One deliberate slow thing.** A single expensive, quiet moment per night — the beach regardless of events. Contrasts make the loud scenes loud.

---

## 9. Effort, risk and verification summary

| Scene | Tier | New tables | New RPCs | New commands | Headless-verifiable today | Effort | Verdict |
|---|---|
| SC-1 Occupied club | T3 | none | none | none | **yes** | M | **Do first** |
| SC-4 Opportunities | T4 | none | none | none | **yes** | S/M | **Do first** |
| SC-5 Weather street | T1 | none | none | none | partly (reads clock) | S | Quick win, visuals only |
| SC-6 Beach by period/events | T1/T4 | none | clock seam only | none | with P4/P5 | M | Phase 2 |
| SC-7 Apartment aftermath | T5 | none (scope to messages/receipts) | none | none | **yes** | S/M | Phase 3 |
| SC-2 Suya queue | T2/T3 | none (derived from presence) | queue read (or derive client-side from presence) | none | **yes** | M/L | Phase 3 |
| SC-3 DJ booth | T5 | none | none | none | yes with AI fixtures | M | Phase 4 |
| SC-8 Restaurant table | T2/T5 | none | none | none | yes | M | Phase 5 |
| SC-10 Cross-engine window | — | none | none | none | after ME-2/3/4 | S then | ME-4 target |
| SC-9 Careless circuit | T2/T4 | **reputation** | several | new command type | no | L | **Blocked** |

**Zero requires a new migration except SC-9** — that is the strongest signal this sequence is realistic.

---

## 10. Risks specific to scene work

| # | Risk | Mitigation |
|---|---|---|
| S1 | Scenes built art-first, then discovered unverifiable | §7 items 7–8 are mandatory, not optional |
| S2 | Presence silently becomes authority | §2 rule 5 + §7 item 4 reviewed per PR |
| S3 | Scene PRs bundle new command types | §2 rule 2: new command type = separate approved migration |
| S4 | AI latency/cost makes a venue feel broken | SC-3 deterministic fallback lines from `npc_dialogue_lines` |
| S5 | Time-gated content unverifiable in CI | P5 clock seam |
| S6 | Event seeds expire and stay expired | P4 |
| S7 | Scene IDs and command vocabulary drift like the 3-copy problem | P6 + anti-drift test |
| S8 | Art scope creep (paid asset packs, large downloads) | placeholder art until a scene passes its headless test |
| S9 | "Sophisticated" becomes a reason to skip the Android-first budget | per-PR performance note (§7.10) |

---

## 11. Acceptance criteria for this plan

- [x] Every proposal states what *already exists* vs what is missing, with file evidence.
- [x] **No scene requires a new migration** except the one explicitly marked **blocked**.
- [x] Ranking is by *payoff ÷ new truth created*, not by visual ambition.
- [x] Every recommended scene is headless-verifiable or has a named prerequisite that makes it so.
- [x] Verification limits of this environment are stated up front (§5 Phase 0 P2).
- [ ] **Not claimable:** that any scene will look good. That is an art judgement requiring a rendered frame, which this environment cannot produce today.

---

## 12. Changes made by this document

| File | Change |
|---|---|
| `docs/NAAD_CREATIVE_SCENES_PLAN.md` | **Added** — this plan |

No other file was created, edited, or deleted. No scene, prefab, migration, contract or asset was touched.
