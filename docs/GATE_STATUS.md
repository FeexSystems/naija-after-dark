# NAAD Gate Status

Last audit: 2026-10-23
Branch: `main`
Unity editor (actual): `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`)
Note: older bible drafts mentioning `6000.0.32f1` are outdated; Gate 22 docs already pin `6000.6.5f1`.

Status key: Planned · In progress · Implemented · Verified · Blocked

## Infrastructure Improvements (2026-10-23)

|| Item | Status | Evidence |
|---|---|---|
| Project LICENSE file | Implemented | Apache-2.0 license added (LICENSE file) |
| Third-party license documentation | Implemented | LICENSE_DOCS.md created documenting all third-party assets |
| Backend build configuration | Implemented | package.json and tsconfig.json added for backend workspaces |
| Backend README updates | Implemented | Build and test commands documented in backend READMEs |
| Root AGENTS.md expansion | Implemented | Comprehensive agent development guide added |
| Git submodule initialization | Implemented | unity-cloud-asset-manager-for-blender submodule initialized |
| GitHub Actions secrets documentation | Implemented | GATE_22.md updated with secrets setup instructions |
| CI secrets configuration | Pending | User must configure UNITY_EMAIL, UNITY_PASSWORD, NAAD_TEST_EMAIL, NAAD_TEST_PASSWORD in GitHub Actions |

Full roadmap: [PROJECT_PLAN.md](PROJECT_PLAN.md) · Next-gate plan: [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md)

## Gate A — Project audit and Unity foundation

| Item | Status | Evidence |
|---|---|---|
| Repo / branch inspection | Verified | `main` at `272b7ec`; CI-recovery fixes on branch `cline/febyxe77` (origin `8f64c6c` pushed; workflow image edit local-only at `5fba087`, pending `workflows` permission) |
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
| CI editor image / workflow version | Blocked | `unityVersion: 6000.7.0b4` has no published `unityci/editor` image — run 39908764911 failed at `docker pull` (manifest unknown, exit code 125). Fix ready: `6000.6.5f1` (active `linux-il2cpp` images, matches all project docs). Workflow file cannot be pushed by the CI identity without `workflows` write permission; fix is committed locally only (`5fba087`). |

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
| CI green run (`verify-unity`, `verify-project-structure`, `first-night-live-test`) | **Pending — blocked by workflow permission** | Run 39908764911 FAILED at ~3s: `unityci/editor:ubuntu-6000.7.0b4-linux-il2cpp-3` manifest unknown, exit code 125. Fix: workflow `unityVersion` → `6000.6.5f1` (committed locally only, `5fba087`; cannot push without `workflows` write permission). Run URL + SHA to be recorded once CI is green. |

## Next gate and exit criteria

Gate: **First Night runtime verification in CI** (extends Gate B). Measurable exits:

1. Green CI run on `feat/audit-gate-recovery-2026-10-14` — record run URL + commit SHA here
   once green (unblocks Gate 22 for real). Repository Actions secrets `UNITY_EMAIL` +
   `UNITY_PASSWORD` and `NAAD_TEST_EMAIL` + `NAAD_TEST_PASSWORD` are required.
   - Run URL: `https://github.com/FeexSystems/naija-after-dark/compare/main...feat/audit-gate-recovery-2026-10-14?expand=1`
   - Commit SHA: `aeaf248` (branch head at time of push)
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
