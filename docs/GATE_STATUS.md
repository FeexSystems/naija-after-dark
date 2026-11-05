# NAAD Gate Status

> **This is the single live ledger.** `PROJECT_PLAN.md` holds the roadmap only, and
> `PRODUCTION_READINESS_STATUS.md` is a historical snapshot of the 2026-10-29 PR #16
> run. When a gate lands, this file is updated in the same commit — that discipline is
> what the previous drift was missing.
>
> Current workstreams and sequencing: [IMPLEMENTATION_PLAN_REMAINING.md](IMPLEMENTATION_PLAN_REMAINING.md).

Last audit: 2026-11-05
Branch: `main`
Unity editor (actual): `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`)
Note: older bible drafts mentioning `6000.0.32f1` are outdated; Gate 22 docs already pin `6000.6.5f1`.

Status key: Planned · In progress · Implemented · Verified · Blocked

## ⛔ BACKEND LOST — 2026-11-05

**The Supabase project `unzfqrfyejkyisalzkhc` no longer exists.**

```
$ nslookup unzfqrfyejkyisalzkhc.supabase.co 8.8.8.8
*** dns.google can't find unzfqrfyejkyisalzkhc.supabase.co: Non-existent domain
$ nslookup supabase.com 8.8.8.8
Name:    supabase.com            ← control resolves fine
```

This is a deleted project, not a network or DNS-filtering problem.

**What it invalidates:**

- Every First Night command fails at runtime. `SupabaseConfig.IsConfigured` is true,
  so the client composes *live* services and every RPC call targets a dead host.
- The **`GATE_B_PASS` / `PHASE_D_PASS` evidence below is historical only.** It
  records a run that happened; it is not reproducible today.
- The `first-night-live-test` CI job cannot pass until a new project exists.

**Recovery:** [SUPABASE_RESTORE_RUNBOOK.md](SUPABASE_RESTORE_RUNBOOK.md) — create a
project, paste `supabase/apply_all.sql`, add a user, update `SupabaseConfig.asset`,
export `NAAD_DEV_EMAIL`/`NAAD_DEV_PASSWORD`, verify with `tools/verify-supabase.py`.

Applying the bundle will be the **first execution of migrations 0010 and 0011**,
which redefine `naad_spend`, `naad_buy` and `naad_execute_command`, and revoke
`naad_react_to_event_attendance` from the public roles.


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

**Contract verified 2026-11-05** (`VerifyAnimationAssets` 37/37, 0 failures, run
against the Editor in `game/unity`).

What changed since the 2026-11-02 entry, all verified by reading the tree rather
than the docs:

| Claim (2026-11-02) | Reality (2026-11-05) |
|---|---|
| "Real rig/clip/controller/prefab assets are still absent" | **False.** `3b0749c` (2026-11-02, on `main`) replaced all eight clips with real Mixamo takes — 672,477 lines of muscle curves. |
| Eight clips are placeholders | **False.** All eight are real takes; `Hold` and `PutAway` are *derived* from real takes (no dedicated source exists). |
| "the contract job proves wiring" | It did not. CI ran `ConfigureAndVerify`, which **regenerated** the assets before checking them, so it could pass while the committed prefab had `m_Controller: {fileID: 0}`, zero MonoBehaviours, and a controller with **zero states and zero motions**. |

Repaired 2026-11-05:

- `PlayerController.controller` — rebuilt: 14 states, 14 bound motions, layers
  `Base Layer` / `Social` / `Phone` / `Seating`.
- `ConfigureGateCAssets.EnsureLayer` — **real bug fixed.** It mutated a layer then
  located its slot with `Array.IndexOf(controller.layers, layer)`; because every
  `controller.layers` read returns a fresh array, the lookup returned `-1` and
  `ReplaceLayer` silently no-opped, so every layer persisted `m_Mask: {fileID: 0}`.
  That is why `UpperBodyMask` never applied and verification failed on it.
- `PlayerRig.prefab` — now carries `CharacterAnimationController`,
  `AnimationStateManager`, `PlayerLocomotionController`, `CharacterController`, an
  Animator bound to `PlayerController`, and a valid humanoid `PlayerAvatar`.
- `PlayerAvatar.asset` — created (it did not exist; the prefab previously borrowed
  an avatar from an *untracked* Mixamo FBX).
- CI `verify-animation` now runs `VerifyAnimationAssets.RunVerification` (verify
  only, no regeneration), so a green run proves the committed assets are correct.

Still open: play-test motion (the contract proves wiring, not movement), and
`Animations/NAADASSETSNEW/` (~77 MB of source takes) is uncommitted, so
`ImportMixamoClips.Run` cannot regenerate from a clean checkout.

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
| CI green run (`verify-unity`, `verify-project-structure`, `verify-scene-registration`, `verify-animation`) | **Verified (2026-10-25)** | Merge PR #14 (commit `14fe069`) — all *scheduled* CI jobs passed. Run URL: https://github.com/FeexSystems/naija-after-dark/actions/workflows/naad-unity.yml |
| `first-night-live-test` in CI | **Not verified — job is skipped on PRs** | The job is gated on `workflow_dispatch` + `inputs.run_live_first_night_test`, so it does **not** run on push or pull_request. It must be dispatched manually. Do not read a green PR run as live CI coverage of First Night. |

## Next gate and exit criteria

Gate: **Gate C — Player Rig & Animation** (next priority). See [GATE_C_PLAYER_RIG_ANIMATION.md](GATE_C_PLAYER_RIG_ANIMATION.md) for full implementation plan.

Previous gate completed: **First Night runtime verification in CI** (extends Gate B).
- ✅ Green CI run on main via PR #14 (commit `14fe069`) for the compile/structure/scene/animation jobs
- ✅ Repository Actions secrets configured (UNITY_EMAIL, UNITY_PASSWORD, NAAD_TEST_EMAIL, NAAD_TEST_PASSWORD)
- ⚠️ `first-night-live-test` was **not** part of that green run — it is manual-dispatch only
- ✅ Run URL: https://github.com/FeexSystems/naija-after-dark/actions/workflows/naad-unity.yml
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
