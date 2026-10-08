# NAAD Gate 21 — Unity Integration & Playable Vertical Slice

## Objective
Turn the existing NAAD Unity client into a reproducible project that can be opened, compiled, verified, and eventually built for Android from a clean clone.

## Scope
- Unity 6000.0.32f1
- Assets/Scenes/Bootstrap.unity
- Build Settings index 0 = Bootstrap
- Bootstrap can remain in the same scene when nextSceneName is empty
- First Night controller and HUD are present in Bootstrap
- Unity batch verification via NAAD.Editor.NAADBuild.Verify
- Optional Android build via NAAD.Editor.NAADBuild.BuildAndroid
- Gemini model is configurable through GEMINI_MODEL

## Local verification
Run Unity in batch mode with:

    Unity -batchmode -nographics -quit -projectPath game/unity -buildTarget StandaloneLinux64 -executeMethod NAAD.Editor.NAADBuild.Verify -logFile -

On Windows, replace Unity with the Unity Editor executable installed by Unity Hub.

## Android verification
After Android Build Support is installed:

    Unity -batchmode -nographics -quit -projectPath game/unity -buildTarget Android -executeMethod NAAD.Editor.NAADBuild.BuildAndroid -logFile -

Expected artifact: game/unity/Builds/Android/NAAD.apk

## GitHub Actions
The Unity workflow verifies the project on pushes and pull requests affecting game/unity.

Required repository secrets:
- UNITY_EMAIL
- UNITY_PASSWORD
- UNITY_LICENSE

Android build is opt-in through NAAD_ANDROID_BUILD=true.

## Gate evidence
Gate 21 is NOT PASS until:
1. Clean Unity import succeeds.
2. NAAD.Editor.NAADBuild.Verify exits successfully.
3. Bootstrap opens without compile errors.
4. Auth restores or signs in.
5. Player and world state load.
6. First Night command flow executes.
7. Live Tunde dialogue returns with a supported Gemini model.
8. Android build installs on a physical device.
9. First Night reaches Night Summary on-device.

Do not mark the gate passed from source inspection alone.