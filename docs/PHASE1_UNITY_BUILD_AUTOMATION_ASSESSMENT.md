# Phase 1 Unity Build Automation Assessment

- Record ID: P1-003
- Date: 2026-10-22
- Repository: FeexSystems/naija-after-dark
- Branch: chore/phase1-pre-edit-decision-record
- Related PR: https://github.com/FeexSystems/naija-after-dark/pull/11
- Scope: Verify Unity Build Automation (UBA) as an alternative CI route for exact Unity Editor 6000.7.0b4.
- Status: Documentation review completed; Dashboard version availability and an actual build remain unverified. No UBA target created, no build triggered, no secrets changed, no deployment.

## Finding

Unity's public Supported Unity Versions page states:
- UBA does not support Unity Alpha versions.
- UBA supports the latest Unity Beta version as soon as possible.
- UBA deprecates a Beta version when it is officially released.

The published table lists 6000.3 LTS, 6000.0 LTS and other supported/deprecated/available versions, but does not explicitly list 6000.7 or 6000.7.0b4. The page was last updated three months ago. Its omission is not sufficient evidence that the current beta is unavailable, but the public policy is also not sufficient proof that the exact patch version is selectable today.

**Conclusion: exact-version UBA support is UNCONFIRMED until the Unity Dashboard's version selector and a real build establish it.** Do not substitute a different editor version.

## Configuration path if the exact version is selectable

Create a separate, manual-only validation target in Unity Dashboard > DevOps > Build Automation > Configurations. Do not enable automatic builds.

- Target name: NAAD-PHASE1-6000.7.0b4-VALIDATION
- Source control: connect the existing Git repository using the least-privileged read-only access supported.
- Branch: chore/phase1-pre-edit-decision-record
- Project subfolder path: game/unity
- Unity version: exactly 6000.7.0b4; do not use auto-detect as a substitute for verifying the selected resolved version.
- Builder OS/platform: select a supported Linux Standalone configuration for the initial compile/build proof, subject to the version/platform choices offered by UBA.
- Auto-build: Off.
- Scheduling: None.
- Signing credentials: None for the initial unsigned validation.
- Deployment/release integration: None.
- Live First Night test: Do not configure or run it against the production/live Supabase project.
- Secrets: do not put license credentials, Supabase service-role keys, production tokens, or user passwords in repository files or build scripts. Only add a secret if the specific build operation requires it and its scope is understood.

Before triggering the build, capture a screenshot or export of the target configuration showing the exact editor version, branch, project subfolder, platform, and disabled auto-build setting. After the build, record the UBA build ID, source revision, resolved Unity version, result, logs, and package-resolution diagnostics.

## Important capability boundary

UBA is a cloud build service, not automatically a drop-in runner for every custom GameCI buildMethod. The existing workflow invokes custom editor methods such as:

- NAAD.Editor.NAADBuild.Verify
- NAAD.Editor.Editor.VerifyProject
- NAAD.Editor.VerifyAnimationAssets.RunVerification
- NAAD.Editor.Editor.ListPackages
- NAAD.Editor.FirstNightLiveTest.Run

A standard UBA build target may build a player without executing those exact custom verification entry points. Before claiming parity with the current GitHub Actions checks, establish a supported UBA test/script-hook path for the verification methods and confirm which reports/logs can be retained. Do not treat a successful player build as proof that the package inventory, Gate C animation contract, or First Night tests passed.

## Verification gates

1. Dashboard gate: confirm exact 6000.7.0b4 appears as a selectable UBA editor version for the target.
2. Source configuration gate: confirm branch and subfolder are correct, auto-build is disabled, and no signing/deployment path is configured.
3. Manual build gate: trigger one unsigned Linux Standalone validation build against the recorded branch commit.
4. Exact-version gate: verify the build logs report Unity 6000.7.0b4, not merely a version family or fallback.
5. Package/import/compile gate: inspect actual package-resolution and compiler logs. A failed build is a blocker to classify, not a pass.
6. Verification parity gate: separately prove how each custom verification method is run before treating UBA as a replacement for existing GameCI checks.
7. Security gate: confirm no live Supabase mutation, production deployment, secret leakage, or signing credentials were involved.

## Stop conditions

- If 6000.7.0b4 is absent from the Dashboard selector, stop and capture the available version list; do not pick another editor.
- If the exact version is selectable but a build fails before Unity starts, capture the failure and ask Unity support/UBA support whether that beta build is available in the selected builder region/platform.
- If Unity launches but package resolution fails, classify package IDs, registries, manifest/lock consistency, and editor compatibility separately.
- Do not turn on auto-build, connect deployment, merge PR #11, modify main, or run the live First Night test.
- Do not remove MCP packages without separate code/config/tool-usage evidence.

## Public documentation reviewed

- Supported Unity versions: https://docs.unity.com/en-us/build-automation/reference/supported-unity-versions
- Configure a build: https://docs.unity.com/en-us/build-automation/basic-build-configuration/overview
- Unity Build Automation overview: https://docs.unity.com/en-us/build-automation

## Evidence still required

- Actual Unity Dashboard version-selector evidence for 6000.7.0b4.
- Confirmation that the project's Git connection can read the validation branch and submodule content as needed.
- One manual build result and its logs.
- A tested route for running the project's custom verification methods under UBA.
