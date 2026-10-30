# NAIJA AFTER DARK — Production-Readiness Status Report

**Repository:** `FeexSystems/naija-after-dark`  
**Pull request:** #16  
**Working branch:** `chore/production-readiness-foundation`  
**Latest commit:** `1a82e4b` — `fix(unity): align verifier paths with Supabase config layout`  
**Unity editor:** `6000.6.5f1`  
**Report date:** 2026-10-29  
**Overall status:** **Not production-ready; core CI compilation is green, but Gate C asset readiness and downstream verification remain incomplete.**

## Executive summary

The production-readiness work has materially improved the repository. The Unity project now passes the main compilation and Gate 22 bootstrap verification in CI, the backend test/build workflow passes, Unity UI and package-inventory checks pass, and Unity metadata validation is clean across 542 `.meta` files.

The project is not yet ready for production release. The strict Gate C humanoid/animation contract fails because the clean repository checkout does not contain the required player rig, prefab, Animator controller, Avatar, AvatarMask, or animation clips. In addition, the most recent completed downstream run exposed incorrect hard-coded paths in the project-structure and service verifiers. Those paths were corrected in commit `1a82e4b`; the corresponding Unity workflow is currently running and has not yet produced a final result.

The live First Night end-to-end test is intentionally not an automatic pull-request gate. It is opt-in because it mutates live Supabase test-account state and requires dedicated credentials. It was skipped in the latest completed PR run, so its current CI status is not verified by this PR.

## Current CI snapshot

The latest workflow for commit `1a82e4b` is currently in progress:

- [NAAD Unity Gate 22 run 40814263042](https://github.com/FeexSystems/naija-after-dark/actions/runs/40814263042): **in progress**
- [Backend checks run 40814262790](https://github.com/FeexSystems/naija-after-dark/actions/runs/40814262790): **passed**
- [Pull request #16](https://github.com/FeexSystems/naija-after-dark/pull/16)

The latest fully completed Unity run was commit `8792e6d`, workflow [40810856997](https://github.com/FeexSystems/naija-after-dark/actions/runs/40810856997). Its results were:

- Unity compile and Gate 22 verification: **passed**
- Unity UI verification: **passed**
- Unity package inventory: **passed**
- Gate C humanoid rig and Animator contract: **failed**
- Unity project structure verification: **failed** because it looked for `Assets/NAAD/Networking/SupabaseConfig.cs`, while the real file is under `Assets/NAAD/Networking/Config/SupabaseConfig.cs`
- Unity service verification: **failed** for the same path mismatch
- First Night live end-to-end: **skipped** by design
- Backend checks: **passed**
- GitGuardian security checks: **passed**

## Gate-by-gate status

| Gate / area | Status | Evidence and remaining work |
|---|---|---|
| **Repository and Unity foundation** | **Implemented / mostly verified** | Unity version is pinned to `6000.6.5f1`; metadata validation passes; the main Unity compile and `NAAD.Editor.NAADBuild.Verify` pass in CI. The branch contains workflow diagnostics and production-readiness fixes. |
| **Unity metadata integrity** | **Verified** | `tools/validate-unity-meta.py game/unity` checked 542 `.meta` files and passed. The known malformed GUID was repaired and a CI metadata gate was added. |
| **Unity compilation** | **Verified** | The main Unity verification job passed in completed run `40810856997`, after fixing editor-script compilation errors in `CreateScreenPrefabs`, `ModelValidator`, `RunAllTests`, and `NaadCityStreetBuilder`. |
| **Gate 22 bootstrap verification** | **Verified in latest completed run** | `NAADBuild.Verify` passed, including bootstrap scene loading, configured `SupabaseConfig`, First Night wiring, HUD/controller wiring, and MainCamera validation. |
| **Project structure verification** | **Pending rerun** | The prior failure was a verifier defect: it checked the wrong `SupabaseConfig.cs` path. Corrected in `1a82e4b` to `Assets/NAAD/Networking/Config/SupabaseConfig.cs`. Awaiting workflow result. |
| **Service configuration verification** | **Pending rerun** | The prior failure had the same incorrect path. Corrected in `1a82e4b`; service interfaces and Supabase service source are present. Awaiting workflow result. |
| **Gate B — scenes/environment** | **Verified locally; CI surface partly dependent on downstream jobs** | Apartment, CityStreet, SuyaSpot, Nightclub, and Beach scenes were authored/registered and previously validated headlessly. The project plan records Gate B environment validation as verified. |
| **Gate C — player rig and animation** | **Blocked** | The strict verifier correctly fails because required production assets are absent from the clean checkout. Required assets include `PlayerRig.fbx`, `PlayerRig.prefab`, `PlayerController.controller`, `PlayerAvatar.asset`, `UpperBodyMask.mask`, and eight required animation clips. |
| **Gate D — gameplay/interaction** | **In progress** | First Night controller/HUD and backend-backed interaction path exist. A complete third-person controller and production interaction framework are not yet verified end-to-end. |
| **Gate E — atmosphere/simulation** | **Planned** | `WorldClockPresenter` exists, but day/night lighting profiles, crowd systems, and broader atmosphere simulation are not verified. |
| **Gate F — mobile UI/release** | **In progress** | Auth UI and First Night HUD exist; Unity UI verification passes. Android/iPhone device acceptance, performance profiling, battery/thermal validation, and release packaging are not complete. |
| **First Night live end-to-end / Phase D** | **Locally verified; CI intentionally skipped in PR** | The project documents local verification of sign-in, player fetch, start-night, phone reply, travel, spending, completion, summary shape, invalid SKU rejection, idempotency, and same-location protection. Automatic CI execution is disabled because it mutates live Supabase state; a staging project and disposable credentials are required before making it a required gate. |
| **Backend tests/build** | **Verified** | Backend checks pass on the latest commit. Workspace build configuration, type checking, tests, lint configuration, and dependency tooling were repaired during this work. |
| **Security scanning** | **Verified for current PR checks** | GitGuardian passed. GitHub reported one high dependency vulnerability on the default branch; it remains an operational remediation item and should be reviewed in Dependabot. |
| **Source-control / review hygiene** | **In progress** | Production-readiness changes are isolated on PR #16. The PR still requires final downstream CI results and human review before merge. |

## Changes completed on the production-readiness branch

The branch includes the following categories of work:

1. Added a dependency-free Unity metadata validator and repaired the known invalid GUID.
2. Added independent backend test/build CI and corrected backend TypeScript workspace configuration.
3. Added a Unity metadata gate and preserved Unity editor/compiler diagnostics in CI artifacts and job output.
4. Fixed Unity compile blockers across gameplay, UI, dialogue-service contracts, editor scripts, test-runner namespace usage, and duplicate city-builder helpers.
5. Added compatibility APIs for NPC state handling, dialogue bubbles, UI service/controller wiring, and Task-based dialogue loading.
6. Isolated obsolete editor-authoring utilities behind `NAAD_LEGACY_EDITOR_TOOLS` so stale optional tooling cannot block the production verification assembly.
7. Corrected project-structure and service-verification paths for the actual `SupabaseConfig` location in commit `1a82e4b`.

## Gate C blocker details

The strict Gate C verifier is intentionally not a file-existence-only check. It requires:

- A real imported humanoid FBX with a valid humanoid Avatar.
- A player prefab containing Animator, CharacterAnimationController, AnimationStateManager, CharacterController, and PlayerLocomotionController.
- A PlayerController Animator Controller.
- A valid UpperBodyMask AvatarMask assigned to a non-base Animator layer.
- Typed Animator parameters: `LocomotionSpeed`, `LocomotionState`, `SocialState`, `PhoneState`, `SeatingState`, and `Trigger`.
- Eight usable clips: locomotion Idle/Walk/Run, phone Retrieve/Hold/Browse/PutAway, and social Greeting.
- Controller references to the required clips and prefab wiring to the controller/avatar.

The repository currently contains the animation scripts and documentation but none of the expected production asset files. The project’s `ASSET_ACQUISITION.md` describes Mixamo, Unity Starter Assets, or licensed custom production as possible acquisition paths. These assets must be imported with stable `.meta` files and then re-run through the strict verifier.

## Release-readiness assessment

**Current release decision: NO-GO.**

The project has reached a credible technical foundation and a reproducible green Unity compile, but it is still a vertical-slice development build rather than a production-ready release. The decisive blockers are:

1. Gate C player rig and animation assets are absent.
2. The latest verifier-path correction is still awaiting CI confirmation.
3. Device-level mobile acceptance and performance profiling are incomplete.
4. Live First Night CI is not currently repeatable in a safe isolated staging environment.
5. The GitHub-reported high dependency vulnerability on the default branch requires review.
6. Gate D/E/F scope remains incomplete: full interaction, atmosphere/simulation, and release/device validation are not finished.

## Recommended next actions, in priority order

### Immediate

1. Let workflow `40814263042` finish and confirm that project-structure and service verification pass on commit `1a82e4b`.
2. Review the workflow artifact and PR checks for any new downstream failures.
3. Review and remediate the reported high dependency vulnerability through Dependabot or a targeted dependency upgrade.

### Gate C completion

1. Select a licensed humanoid source, preferably the documented free Mixamo or Unity Starter Assets route.
2. Import the rig, clips, controller, Avatar, and AvatarMask into the exact paths expected by `VerifyAnimationAssets`.
3. Commit all binary assets and stable `.meta` files.
4. Configure the Animator parameter and overlay-mask contract.
5. Run the strict Gate C job until all checks pass.
6. Perform a real movement/phone/social animation playback test after the headless contract passes.

### Production hardening

1. Create a disposable staging Supabase project and isolated test account for the First Night CI test.
2. Enable the live First Night workflow only against staging, with resettable state and dedicated secrets.
3. Complete Gate D interaction integration over validated `SceneAnchor`s.
4. Implement and verify Gate E lighting, world-clock presentation, and crowd/atmosphere systems.
5. Profile the game on representative Android and iPhone devices before optimizing.
6. Run a release build, install/launch smoke tests, save-state tests, network failure tests, and mobile performance acceptance.

## Evidence links

- [Pull request #16](https://github.com/FeexSystems/naija-after-dark/pull/16)
- [Latest in-progress Unity workflow — run 40814263042](https://github.com/FeexSystems/naija-after-dark/actions/runs/40814263042)
- [Latest completed Unity workflow — run 40810856997](https://github.com/FeexSystems/naija-after-dark/actions/runs/40810856997)
- [Latest backend workflow — run 40814262790](https://github.com/FeexSystems/naija-after-dark/actions/runs/40814262790)
- [Gate status ledger](docs/GATE_STATUS.md)
- [Project plan](docs/PROJECT_PLAN.md)
- [Gate C player rig and animation plan](docs/GATE_C_PLAYER_RIG_ANIMATION.md)
- [Animation asset acquisition guide](game/unity/Assets/NAAD/Animation/ASSET_ACQUISITION.md)

## Final conclusion

NAIJA AFTER DARK is **technically healthier and substantially closer to production readiness**, with the main compile and bootstrap gate now green and backend CI passing. It should not yet be labeled production-ready or released. The next decisive milestone is a green Gate C verification after importing the real licensed player rig and animation assets, followed by device-level mobile acceptance and a safe staging-based live First Night test.
