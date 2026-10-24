# NAAD Gate Status

Last audit: 2026-10-14 (version/branch header corrected 2026-10-24)
Branch: not pinned here — run `git rev-parse --abbrev-ref HEAD`. The 2026-10-14 audit ran on
`feat/audit-gate-recovery-2026-10-14`; gate work later continued on other branches.
Unity editor (actual): `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`) — also
pinned by all jobs in `.github/workflows/naad-unity.yml`.
Note: `6000.7.0b4` is installed on some workstations as a second editor, but it is **not** a gate
editor: headless batch mode on it hangs at `Application.AssetDatabase Initial Refresh Start`
(see the Unity CLI Pipeline row under Gate A). Older gate drafts mentioning `6000.0.32f1` are
historical.

Status key: Planned · In progress · Implemented · Verified · Blocked

Full roadmap: [PROJECT_PLAN.md](PROJECT_PLAN.md) · Next-gate plan: [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md)

## Gate A — Project audit and Unity foundation

| Item | Status | Evidence |
|---|---|---|
| Repo / branch inspection | Verified | On feature branch `chore/unity-mcp-scaffolding-and-gitignore`; 1 commit ahead of `origin/chore/unity-mcp-scaffolding-and-gitignore` |
| Core service graph | Implemented | `NAADApplicationRoot`, stubs + Supabase live services, command/auth/player/world/phone/dialogue |
| Bootstrap scene in Build Settings | Verified | `Assets/Scenes/Bootstrap.unity` is at build index 0; five additional canonical NAAD scenes follow it |
| Bootstrap First Night wiring | Implemented | `NAADBootstrap`, `FirstNightController`, `FirstNightHud`, `MainCamera` present |
| SupabaseConfig assigned on root | Verified | Wired to `Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset` |
| Bootstrap MainCamera component | Verified | Tagged `MainCamera` object was missing `Camera` component; added and saved |
| `NAADBuild.Verify` (Gate 22) | Verified | Local Editor: `VERIFY_PASS` via `NAAD.Editor.NAADBuild.Verify` |
| Production scene registry (Apartment/Street/Suya/Club/Beach) | Verified | Apartment/CityStreet/SuyaSpot/Nightclub/Beach are registered after Bootstrap in `EditorBuildSettings.asset`; prior headless validation used `NaadScenePipeline.ValidateScenes` (exit 0) |
| Scene transition after Bootstrap | Planned | Serialized `nextSceneName` is empty (First Night currently lives in Bootstrap) |
| Unity CLI Pipeline drive | Blocked pending editor startup | Unity `6000.7.0b4` headless batch mode hangs at `Application.AssetDatabase Initial Refresh Start` and never reaches `-executeMethod`. Fix pending: see `gatec-fresh.log`/`gatec-gui.log` in `game/unity/Logs/`. |
| Compile-clean Editor | Verified | Console cleared after Input System 1.20.1 + package cleanup |

## Gate B — Environment / scenes

Verified. Six Environment placeholder prefabs authored
(`NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders`, 6/6) and five NAAD
scenes built headless via `NAAD.Editor.NaadScenePipeline.BuildApartmentAndCityStreet`:
Apartment, CityStreet, SuyaSpot, Nightclub, Beach. `ValidateScenes` PASS (exit 0).
Vendor packs remain optional (G3).

## Gate C — Characters / animation

**In progress; not release-ready.** Added `PlayerLocomotionController` with script-driven, camera-relative movement and input injection for mobile joystick/run controls. Extended `VerifyAnimationAssets.RunVerification` into a strict headless contract check and added a `verify-animation` CI job. The check now requires a valid Humanoid FBX/avatar, eight MVP clips, typed Animator parameters, an upper-body mask layer, and a wired player prefab containing Animator, animation bridge/state manager, CharacterController, and locomotion controller.

**Hard blocker:** real rig/clip/controller/prefab assets are still absent from the clean repository checkout. The strict job is expected to fail until licensed assets are imported and committed with stable `.meta` files. No Unity Editor compile or animation playback test has been executed by this change. Full exit criteria: [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md).
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

## First Night runtime verification (extends Gate B / Phase D) — 2026-10-14

The First Night vertical slice is driven end-to-end by
`NAAD.Editor.FirstNightLiveTest.Run` against the LIVE Supabase project
(`unzfqrfyejkyisalzkhc`): sign-in → fetch player → balance → live graph probe →
`reset_home` → START_NIGHT → phone reply → TRAVEL+SPEND (suya) → TRAVEL+TICKET
(club) → COMPLETE_NIGHT → summary, all commands routed through
`ICommandService` → `naad_execute_command`.

| Assertion / probe | Status | Evidence |
|---|---|---|
| `assert_money_moved` | Verified (local) | before ₦75,000 → after ₦50,000 |
| `assert_spent_matches_delta` | Verified (local) | spent ₦25,000 == 75,000 − 50,000 |
| `assert_summary_shape` | Verified (local) | transactions 2, peopleMet 1, newConnection "Tunde", trust 70 |
| `GATE_B_PASS` | Verified (local) | `Library/first-night-report.json` ok:true |
| `probe_invalid_sku` | Verified (local) | correctly rejected `SKU_NOT_FOUND` |
| `probe_double_request_id` | Verified (local) | idempotent replay (first + replay both success) |
| `probe_same_location` | Verified (local) | `ALREADY_THERE`, no mutation |
| `PHASE_D_PASS` | Verified (local) | `Library/first-night-report.json` ok:true |
| CI green run (`verify-unity`, `verify-project-structure`, `first-night-live-test`) | **Pending** | run URL + SHA to be recorded after first green Actions run |

## Next gate and exit criteria

Gate: **First Night runtime verification in CI** (extends Gate B). Measurable exits:

1. Green CI run on `chore/unity-mcp-scaffolding-and-gitignore` — record run URL + commit SHA here
   once green (unblocks Gate 22 for real). Repository Actions secrets `UNITY_EMAIL` +
   `UNITY_PASSWORD` and `NAAD_TEST_EMAIL` + `NAAD_TEST_PASSWORD` are required.
   - Run URL: pending first green Actions run on this branch
   - Commit SHA: pending first green Actions run on this branch
2. `FirstNightLiveTest.Run` exits 0 in CI with `GATE_B_PASS` and `PHASE_D_PASS` in
   `game/unity/Library/first-night-report.json` (uploaded as the
   `naad-first-night-report` artifact); money-moved and spent-matches-delta both true.
3. Registry stays in sync: any new scene must appear in `EditorBuildSettings.asset`
   and pass `NaadScenePipeline.ValidateScenes` before the registry references it.

## What this audit deliberately did not do

- Did not invent Apartment/Street/Nightclub scenes.
- Did not replace stub/live service architecture.
- Did not commit secrets or alter anon keys.
- Did not mark Android/device tests as passed.
