# NAAD Gate 22 — Reproducible Unity CI Baseline

## Objective

Make the Unity verification workflow reproducible and ensure its editor version matches the project version. This gate is not considered passed merely because the workflow file is updated.

## Scope

- Project editor version: `6000.6.5f1` (source of truth: `game/unity/ProjectSettings/ProjectVersion.txt`).
- GitHub Actions uses the same editor version.
- Workflow checks licensing configuration before invoking Unity.
- `NAAD.Editor.NAADBuild.Verify` opens the actual Bootstrap scene and checks:
  - build index 0 is enabled and points to Bootstrap;
  - the scene loads and contains no missing script references;
  - `NAADApplicationRoot` exists and has a configured `SupabaseConfig`;
  - `NAADBootstrap`, `FirstNightController`, and `FirstNightHud` exist;
  - the HUD's serialized controller reference points to the scene controller;
  - a camera tagged `MainCamera` exists.
- Compilation and verification output must be retained as CI evidence.

## Required GitHub Actions secrets

### Unity Licensing (Required for all jobs)

Configure one supported licensing route:

- `UNITY_LICENSE`: a valid Unity license file/activation string accepted by GameCI; or
- both `UNITY_EMAIL` and `UNITY_PASSWORD`: credentials for the Unity account used by the workflow.

Do not commit license files, credentials, activation serials, access tokens, or other secrets to the repository. A preflight check only confirms that inputs are present; Unity must still validate the license.

### First Night Test Credentials (Optional, for first-night-live-test job)

For the optional First Night live end-to-end test (Gate B / Phase D), configure:

- `NAAD_TEST_EMAIL`: Valid email for a test account on the live Supabase project
- `NAAD_TEST_PASSWORD`: Valid password for the test account

**Important**: The First Night test mutates wallet/game state on the live Supabase project. It is opt-in via `workflow_dispatch` only and should not be an automatic PR gate. Before making it a required gate, move it to an isolated staging project with a disposable test account.

### Setup Instructions

1. Go to your GitHub repository settings
2. Navigate to **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each required secret:
   - Name: `UNITY_EMAIL` (or `UNITY_LICENSE`)
   - Value: Your Unity account email (or license file)
   - Name: `UNITY_PASSWORD` (if using email/password)
   - Value: Your Unity account password
5. Optionally add test credentials for First Night test:
   - Name: `NAAD_TEST_EMAIL`
   - Value: Test account email
   - Name: `NAAD_TEST_PASSWORD`
   - Value: Test account password

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
