# NAAD Gate Status

Last audit: 2026-10-27
Branch: `chore/unity-mcp-scaffolding-and-gitignore`
Unity editor: `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`) — also pinned by all jobs in `.github/workflows/naad-unity.yml`.

Status key: ✅ **Implemented** · ✅ **Verified** · 🟡 **In progress** · ⬜ **Planned** · 🔧 **Blocked**

Full roadmap: [PROJECT_PLAN.md](PROJECT_PLAN.md) · Next gate: [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md)

## Gate A — Project audit and Unity foundation

**Status: ✅ Implemented**

| Item | Status | Evidence |
|---|---|---|
| Repo / branch inspection | ✅ Verified | On feature branch `chore/unity-mcp-scaffolding-and-gitignore` |
| Core service graph | ✅ Implemented | `NAADApplicationRoot`, stubs + Supabase live services, command/auth/player/world/phone/dialogue |
| Bootstrap scene in Build Settings | ✅ Verified | `Assets/Scenes/Bootstrap.unity` at build index 0 |
| Bootstrap First Night wiring | ✅ Implemented | `NAADBootstrap`, `FirstNightController`, `FirstNightHud`, `MainCamera` present |
| SupabaseConfig assigned on root | ✅ Verified | Wired to `Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset` |
| Bootstrap MainCamera component | ✅ Verified | Now has `Camera` component |
| `NAADBuild.Verify` (Gate 22) | ✅ Implemented | Runs cleanly via `UNITY_CLI_COMMANDS.md` |
| Production scene registry | ✅ Verified | Apartment/CityStreet/SuyaSpot/Nightclub/Beach in `EditorBuildSettings.asset` |
| Unity CLI pipeline | ✅ Implemented | `unity run --command` + `unity test --affected` wired into CI |
| Compile-clean Editor | ✅ Verified | Console cleared after Input System 1.20.1 + package cleanup |

## Gate B — Environment / scenes

**Status: ✅ Verified**

Six environment placeholder prefabs authored (`NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders`, 6/6) and five NAAD scenes built headless via `NAAD.Editor.NaadScenePipeline.BuildApartmentAndCityStreet`: Apartment, CityStreet, SuyaSpot, Nightclub, Beach. `ValidateScenes` PASS (exit 0). Vendor packs remain optional.

## Gate C — Characters / animation

**Status: 🟡 In progress — infrastructure complete, runtime assets pending**

Infrastructure complete: `PlayerLocomotionController` (script-driven, camera-relative movement), `CharacterAnimationController`, `AnimationStateManager`, `AnimationCommand`, `VerifyAnimationAssets` (headless contract check), `GateCAssetBuilder` (asset generation pipeline), `GateC` (unified workflow controller). MVP animation FBX files exist in `Animations/MVP_Only/` (7 files: idle, walk, run, phone animations, social greeting). `verify-animation` CI job wired in workflow.

**Hard blocker:** No `.controller`, `.mask`, `.prefab`, or `.asset` files for player rig exist. `GateCAssetBuilder` can generate but has not been run. No Animator Controller, Avatar, UpperBodyMask, or PlayerRig prefab committed. Verification report (`VERIFICATION_REPORT.md`) confirms infrastructure complete but assets missing. Full exit criteria: [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md).

## Gate D — Gameplay / interaction

**Status: 🟡 Implemented (code complete, scene wiring pending)**

Implementation complete: `PlayerLocomotionController` (script-driven, camera-relative movement), `CharacterAnimationController`, `AnimationStateManager`, `InteractionSystem` (discovery via OverlapSphere, facing-cone aware, HUD integration), `Interactable` (base component), `InteractableTarget` (helper), `PlayerPrefabCreator` (wiring via `NAAD > Characters > Create Player Prefab`). Mobile input support: `VirtualJoystick` (touch joystick with keyboard fallback), `InteractButton` (mobile interact button), `MobileTouchControls` (safe-area handling). Third-person camera: `ThirdPersonCamera` (follow target with smoothing, collision protection). Scene wiring and prefab instantiation pending.

## Gate E — Atmosphere / simulation

**Status: 🟡 Implemented (systems complete, scene integration pending)**

Systems complete: `WorldClockService` (authoritative world clock, period tracking), `WorldClockPresenter` (presentation layer, period-based ambient lighting, demo cycle), `NightLightingProfile` (ScriptableObject for day/night lighting parameters with ambient, directional, skybox, fog, nightlife level). World clock contract verifier in CI (`verify-world-clock` job). Crowd/presence systems remain unverified. Scene integration of lighting profiles pending.

## Gate 9 — Gemini dialogue

**Status: 🟡 In progress**

Edge Function (`supabase/functions/npc-dialogue/index.ts`) implements Gemini dialogue with schema validation. Full on-chain/live deployment requires `GEMINI_API_KEY` to be set in Supabase and the function deployed.

## Gate 10 — NPC memory

**Status: ✅ Verified**

Memory candidate → server commit → dedup within 24h → retrieved via `naad_npc_dialogue_context` (top 5 by importance). All verified via live test.

## Gate 11 — Economy

**Status: ✅ Verified**

SKUs: `SUYA_PLATE` ₦5,000, `TRANSPORT_SHORT` ₦4,000, `CLUB_TICKET` ₦20,000, `BEACH_DRINK` ₦3,000, `SOFT_DRINK` ₦1,500. RPCs `naad_get_wallet` and `naad_spend` verified on live DB.

## Gate 12 — Relationships

**Status: ✅ Verified**

`naad_apply_relationship_delta` — server clamps ±10 per call, scores 0–100. AI only suggests; server applies.

## Gate 13 — Phone

**Status: ✅ Verified**

`naad_seed_tunde_beach_invite` → message with GO / ASK_DETAILS / DECLINE. `naad_respond_message` → records choice + relationship nudge. All verified.

## Gate 14 — First Night

**Status: ✅ Verified**

`naad_start_night` → seed Tunde invite → period NIGHT → player loop (travel, buy, talk) → `naad_complete_night` → summary (spent, people, trust) → return Apartment → AFTER_HOURS.

## Gate 16 — Mobile optimization

**Status: 🟡 Implemented (config complete, device profiling pending)**

Configuration complete: `MobileProfilingConfig` ScriptableObject with targets for low/mid/high Android + iPhone (FPS, RAM, load times, network P95). Mobile input infrastructure complete: `VirtualJoystick` (touch joystick with keyboard fallback, floating behavior, dead-zone), `InteractButton` (mobile interact button, 44dp touch target), `MobileTouchControls` (safe-area handling for notches/bezels, sensitivity config). Actual device profiling on low/mid/high Android and iPhone remains pending device access.

## Gate 17 — Content expansion

**Status: 🟡 Implemented (scaffolding + sample data)**

Content expansion scaffolding complete: DialogueConfig ScriptableObject for NPC dialogue, CareerData ScriptableObject for career content, ContentInitialization for runtime content registration, ContentExpansionUIController for content expansion UI, ContentExpansionAssetCreator editor window. Sample locations (restaurant, lounge, hotel) and careers (tech, creative, hospitality) registered at startup. Full content population pending.

## Gate 18 — Social graph

**Status: ✅ Verified**

`opportunities` table + `naad_refresh_opportunities` (Tunde trust ≥ 55 → "Meet the DJ") + `naad_accept_opportunity`. Verified on live DB.

## Gate 19 — Events

**Status: ✅ Verified**

`world_events` + `event_attendance`. Seeded: Beach Party, Club Night. `naad_list_events` and `naad_attend_event` verified.

## Gate 20 — World reactivity

**Status: ✅ Verified**

Attendance raises `nightlife_level` and `traffic_level`. Domain event `WORLD_REACTIVITY` emitted.

## Summary

| Category | Gates |
|---|---|
| ✅ Implemented and verified | 10, 11, 12, 13, 14, 18, 19, 20 |
| 🟡 In progress (code complete, awaiting assets/conf/scene wiring) | C (rig/assets), D (scene wiring), E (scene integration), 9 (deploy key), 16 (device profiling), 17 (content population) |
| ⬜ Not yet started | 21 (CI green run), 22 (CI workflow green run) |
The project is in a strong intermediate state: the technical foundation, server-authoritative command surface, First Night vertical slice, animation infrastructure, player interaction layer, mobile input systems, atmosphere systems, Gate 17 scaffolding, and all upstream world-model gates are complete. Remaining work is concentrated in asset import (Gate C), scene wiring (Gates D/E), CI secret configuration, and on-device profiling — all familiar, executable next steps.
