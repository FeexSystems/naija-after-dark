# NAAD Gate 22 — Reproducible Unity CI Baseline

## Objective

Make the Unity verification workflow reproducible and ensure its editor version matches the project version. This gate is not considered passed merely because the workflow file is updated.

## Scope

- Project editor version: `6000.6.5f1` (source of truth: `game/unity/ProjectSettings/ProjectVersion.txt`).
- GitHub Actions uses the same editor version.
- Workflow checks licensing configuration before invoking Unity.
- `NAAD.Editor.NAADBuild.Verify` must complete successfully on a clean checkout.
- Compilation and verification output must be retained as CI evidence.

## Required GitHub Actions secrets

Configure one supported licensing route:

- `UNITY_LICENSE`: a valid Unity license file/activation string accepted by GameCI; or
- both `UNITY_EMAIL` and `UNITY_PASSWORD`: credentials for the Unity account used by the workflow.

Do not commit license files, credentials, activation serials, access tokens, or other secrets to the repository. A preflight check only confirms that inputs are present; Unity must still validate the license.

## Verification procedure

1. Confirm the editor version in `ProjectSettings/ProjectVersion.txt`.
2. Confirm the workflow uses the same version.
3. Confirm the licensing preflight passes.
4. Run a clean Unity import and `NAAD.Editor.NAADBuild.Verify`.
5. Inspect the complete workflow log for compiler errors and verification failures.
6. Record the workflow run URL and commit SHA in the PR.

## Exit criteria

Gate 22 is PASS only when the Unity workflow completes successfully against the proposed commit and the logs show successful project import, compilation, and `NAAD.Editor.NAADBuild.Verify` execution.

If licensing is missing or invalid, report the gate as BLOCKED. Do not report source-level inspection as a successful Unity build.
