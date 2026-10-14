# NAAD Gate Status

Last audit: 2026-10-14
Branch: `feat/audit-gate-recovery-2026-10-14`
Unity editor (actual): `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`)
Note: older bible drafts mentioning `6000.0.32f1` are outdated; Gate 22 docs already pin `6000.6.5f1`.

Status key: Planned · In progress · Implemented · Verified · Blocked

## Gate A — Project audit and Unity foundation

| Item | Status | Evidence |
|---|---|---|
| Repo / branch inspection | Verified | On feature branch `feat/audit-gate-recovery-2026-10-14`; `main` at `25498e3` (ahead of reviewed commit by 2 commits) |
| Core service graph | Implemented | `NAADApplicationRoot`, stubs + Supabase live services, command/auth/player/world/phone/dialogue |
| Bootstrap scene in Build Settings | Verified | Only `Assets/Scenes/Bootstrap.unity` at build index 0 |
| Bootstrap First Night wiring | Implemented | `NAADBootstrap`, `FirstNightController`, `FirstNightHud`, `MainCamera` present |
| SupabaseConfig assigned on root | Verified | Wired to `Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset` |
| Bootstrap MainCamera component | Verified | Tagged `MainCamera` object was missing `Camera` component; added and saved |
| `NAADBuild.Verify` (Gate 22) | Verified | Local Editor: `VERIFY_PASS` via `NAAD.Editor.NAADBuild.Verify` |
| Production scene registry (Apartment/Street/Suya/Club/Beach) | Verified | Apartment/CityStreet/SuyaSpot/Nightclub/Beach all registered in `EditorBuildSettings.asset`; built + validated headless via `NaadScenePipeline.ValidateScenes` (exit 0) |
| Scene transition after Bootstrap | Planned | Serialized `nextSceneName` is empty (First Night currently lives in Bootstrap) |
| Unity CLI Pipeline drive | Verified | `unity status` → Editor `ready` on port 7801 (`6000.6.5f1`) |
| Compile-clean Editor | Verified | Console cleared after Input System 1.20.1 + package cleanup |

## Gate B — Environment / scenes

Verified. Six Environment placeholder prefabs authored
(`NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders`, 6/6) and five NAAD
scenes built headless via `NAAD.Editor.NaadScenePipeline.BuildApartmentAndCityStreet`:
Apartment, CityStreet, SuyaSpot, Nightclub, Beach. `ValidateScenes` PASS (exit 0).
Vendor packs remain optional (G3).

## Gate C — Characters / animation

Infrastructure complete, assets missing. Animation pipeline code implemented (AnimationCommand, CharacterAnimationController, AnimationStateManager, VerifyAnimationAssets) with full documentation (ANIMATION_MANIFEST.md, ASSET_ACQUISITION.md, ANIMATOR_CONTROLLER_SETUP.md). No actual animation assets (FBX models, clips, Animator Controllers, prefabs) exist yet — 0/37 MVP assets per VERIFICATION_REPORT.md (2026-10-13).

## Gate D — Gameplay / interaction

In progress (backend-backed First Night controller/HUD exist; no third-person controller or interaction framework in `Assets/NAAD`).

## Gate E — Atmosphere / simulation

Planned. `WorldClockPresenter` exists; no day/night lighting profiles or crowd systems verified.

## Gate F — Mobile UI / AI / release

In progress. Auth UI + First Night HUD exist; phone apps / Android device acceptance not verified in this audit.

## First blocking dependency (Gate A)

1. **Bootstrap must pass `NAAD.Editor.NAADBuild.Verify` with a non-null, configured `SupabaseConfig`.**  
   That assignment was missing (`supabaseConfig: {fileID: 0}`) and is the first foundation blocker for CI/local Gate 22 verification.
2. After Verify passes: missing NAAD production scenes and player locomotion remain the next blockers for Gate B/D — not more package installs.

## What this audit deliberately did not do

- Did not invent Apartment/Street/Nightclub scenes.
- Did not replace stub/live service architecture.
- Did not commit secrets or alter anon keys.
- Did not mark Android/device tests as passed.
