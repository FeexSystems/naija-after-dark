# NAAD Production Readiness Plan

**Last reviewed:** 2026-10-16  
**Scope:** Unity client, imported content, backend integration, CI, mobile release path.  
**Current status:** **Blocked — not production-ready.** This document is a release gate, not a claim that all items are complete.

## Release principle

A successful Unity Editor verification or headless build is necessary but does not prove that the game is production-ready. Readiness requires repeatable clean-clone setup, verified serialized assets, working runtime gameplay, authoritative backend behavior, mobile-device validation, privacy/security review, and a distributable build.

## Current evidence-based status

| Gate | Status | Evidence / blocker | Exit criteria |
|---|---|---|---|
| Unity foundation / Gate 22 | Blocked in CI | CI run 39908764911 failed at ~3s with exit code 125: `unityci/editor:ubuntu-6000.7.0b4-linux-il2cpp-3` image does not exist (manifest unknown). `.github/workflows/naad-unity.yml` declares `6000.7.0b4`; all project docs pin `6000.6.5f1`, for which active images exist. Fix ready (workflow + `ProjectVersion.txt` aligned to `6000.6.5f1`; `Bootstrap.unity` registered in `EditorBuildSettings.asset` at index 0) but the workflow file cannot be pushed without `workflows` write permission. | CI runs green with the corrected editor version; verification report artifact uploaded; run URL + SHA recorded here |
| Project structure verification | Fix applied; pending Unity CI | The implementation is `namespace NAAD.Editor` + `class Editor`; entry point must be `NAAD.Editor.Editor.VerifyProject` | Unity invokes method, report exists, exit code 0 |
| Package inventory | Fix applied; pending Unity CI | Same fully-qualified type-name issue for `ListPackages` | Inventory completes and is stored as an artifact |
| First Night end-to-end | Blocked | Existing test requires `NAAD_TEST_EMAIL` and `NAAD_TEST_PASSWORD`; code comments indicate it mutates a live Supabase project and real wallet balance | Use an isolated staging Supabase project and disposable test account; test must be repeatable and resettable |
| Vendor asset reproducibility | Blocked | Six vendor packs are gitignored; committed Apartment and CityStreet scenes contain vendor GUID references | Clean-clone mode passes with tracked fallback assets; full-art mode explicitly checks vendor packs and GUID resolution |
| Environment art coverage | In progress | Intake mapping: 21 confirmed, 13 partial, 92 unmatched of 126 manifest IDs; these counts include authored placeholders as recorded by the mapping | Required MVP locations have art-approved, correctly scaled, collidable assets; placeholder state is disclosed |
| Player rig and animation | Blocked | `VERIFICATION_REPORT.md` reports no actual rig, clips, controller or player prefab (0/37 MVP assets in that report) | Player prefab, valid Humanoid avatar, idle/walk/run, Animator Controller and interaction animation pass Editor verification |
| Scene navigation / gameplay loop | In progress / unverified | Five environment scenes are registered, but scene registration does not prove runtime transitions | Bootstrap → apartment/street → suya or nightclub → server-backed interaction → Night Summary works in Play Mode |
| Backend security and correctness | Must be revalidated | Server-authoritative command design is documented; production readiness requires policy and transaction evidence | RLS, authorization, idempotency, validation, concurrency, economy invariants and secret boundaries are tested |
| Android build | Not validated in this audit | No device build/install/performance evidence is attached to this change | Reproducible APK/AAB, install on supported physical Android device, smoke test and artifact metadata |
| Release operations | Blocked | No complete signed release candidate, crash/telemetry check, privacy/data-retention review or asset-license dossier confirmed | Versioned candidate, license/provenance records, privacy review, recovery plan and release checklist complete |

## Gate 1 — Repair the Unity CLI entry points

The methods in `Editor.VerifyProject.cs` and `Editor.Packages.cs` are declared inside `namespace NAAD.Editor` and the partial class `Editor`. Their fully qualified type is therefore `NAAD.Editor.Editor`.

Use these commands for those methods:

- `NAAD.Editor.Editor.VerifyProject`
- `NAAD.Editor.Editor.ListPackages`
- `NAAD.Editor.Editor.AddPackage`
- `NAAD.Editor.Editor.ReimportTextures`
- `NAAD.Editor.Editor.VerifyUI` (only where the target method is on that partial class)

Do not change methods on other types: for example `NAAD.Editor.NAADBuild.Verify`, `NAAD.Editor.NaadScenePipeline.ValidateScenes`, and `NAAD.Editor.FirstNightLiveTest.Run` have their own class names.

**Acceptance:** run each changed entry point in Unity 6000.6.5f1 batch mode; capture command, process exit code, log, report existence and report contents. A source edit alone does not pass this gate.

## Gate 2 — Make live integration testing safe

The current First Night live test is not safe as an unconditional pull-request gate while it targets a live project and mutates wallet/game state.

1. Provision a separate Supabase staging project with schema migrations matching the release candidate.
2. Use a dedicated disposable test account and seeded state; do not use a developer or customer account.
3. Ensure the client points to staging through non-secret environment/configuration injection. Never add service-role keys or Gemini keys to Unity client assets.
4. Add deterministic reset/cleanup, bounded test spending, request-ID idempotency assertions, and a unique test run identifier.
5. Keep live-production smoke tests read-only unless a formally approved synthetic transaction and compensating reset exist.
6. Store credentials only in GitHub Actions secrets/environments. Do not log their values.
7. Make missing staging configuration a clearly reported blocked gate, not a false pass.

## Gate 3 — Asset integrity and portability

Keep vendor files out of Git when licenses or repository size make that appropriate, but make the import contract reproducible.

- Record exact product URLs/IDs, publisher, version, acquisition/license evidence and dependencies for each pack.
- Add a portable validation mode that builds using only tracked NAAD assets and fallbacks.
- Add a full-art mode that fails when required vendor roots, scene-critical GUIDs, materials or textures are missing.
- Do not mark scenes visually complete because a primitive fallback allows the build to pass.
- Create a gap list for the 92 unmatched manifest entries; prioritize the playable street block, suya spot and nightclub over optional scenery.
- Review ignored folders that are not in the six-pack intake table before removing or retaining them.
- Verify licenses permit intended commercial use and distribution; do not redistribute source asset packs.

## Gate 4 — Player rig and controls

Implement the minimum coherent player, not a broad animation catalogue:

- Humanoid player model and stable Avatar.
- Player prefab with movement, collision, camera target and animation bridge.
- Idle/walk/run locomotion with validated transitions.
- Mobile touch controls with safe-area handling and configurable sensitivity.
- One phone interaction animation or upper-body gesture.
- Editor validation for all required references and a runtime smoke test in Apartment and CityStreet.

Use licensed assets or project-authored content only. If a suitable rig is not already present locally, acquisition remains an explicit dependency rather than fabricating a claim of completion.

## Gate 5 — First complete vertical slice

The first release-quality loop should be small and end-to-end:

1. Start in Bootstrap and load player/world state.
2. Move around a verified street block.
3. Enter a suya spot or nightclub through a real interaction.
4. Speak to one NPC using bounded, schema-validated AI dialogue.
5. Submit purchase/travel commands to the authoritative server.
6. Confirm wallet and transaction state are server-derived, with idempotent command handling.
7. Return to HUD and show a server-derived Night Summary.
8. Repeat/reload and verify persistence; reject invalid and duplicate commands.

LLM output must never directly mutate wallet, inventory, relationships or world state. It may propose dialogue or a typed intent; the command router validates and authorizes every mutation.

## Gate 6 — Mobile and release verification

- Pin Unity editor and package versions; verify package lock and clean clone.
- Build Android with the documented release method and fail on build errors.
- Install on a physical device; test launch, touch controls, scene transitions, suspend/resume, poor connectivity, auth expiry and backend failure states.
- Measure CPU/GPU frame time, memory, load time and thermal behavior on a representative low/mid-range device.
- Check Android permissions, signing configuration, IL2CPP/ABI settings, app identifier, version, crash diagnostics and release artifact checksums.
- Review accessibility, readable text, safe areas, localization-ready strings, audio levels, privacy, data retention and user reporting/moderation needs.
- Do not call the game release-ready until a candidate build and evidence package are attached to the release record.

## Required evidence package

For each gate, attach logs/reports or device evidence and record:

- commit SHA and Unity version
- exact command or manual test steps
- expected versus observed result
- exit code and test counts
- build artifact path and checksum where applicable
- failures, limitations and owner of any remaining dependency

## Immediate next actions

1. Run CI on this branch and verify the corrected Editor entry points.
2. Replace the live-project test dependency with isolated staging configuration and safe test-state management.
3. Implement full-art GUID validation plus clean-clone fallback validation.
4. Integrate and verify one player rig with locomotion.
5. Complete the street-to-suya/nightclub vertical slice and validate it on Android.
6. Update this document only from test/build/device evidence; never promote a gate based on code presence alone.
