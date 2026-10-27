# NAAD Unity CLI Verification Commands

**Generated**: 2026-10-25
**Purpose**: Automated verification commands for Unity CLI
**Unity Editor**: 6000.6.5f1
**Unity CLI**: Experimental (install from https://docs.unity.com/en-us/unity-cli)

## Prerequisites

### Unity CLI Installation

The Unity CLI must be installed and available in your PATH. If not installed:

```bash
# Download Unity CLI from https://docs.unity.com/en-us/unity-cli
# Add to PATH or use full path to executable
```

### Unity Editor Installation

Unity 6000.6.5f1 must be installed. The CLI will use the installed Editor.

## Project Path

All commands assume the project path is:
```
c:\NAAD\naija-after-dark\game\unity
```

Adjust the path if your project is located elsewhere.

---

## Command Styles

Two command styles are supported. **Modern Unity CLI** (`unity ...`) is recommended for new CI/CD pipelines. **Legacy Editor batch mode** (`Unity.exe -quit -batchmode ... -executeMethod ...`) is retained for compatibility with existing scripts.

| Task | Modern Unity CLI | Legacy Editor Batch Mode |
|------|------------------|-------------------------|
| Run an Editor method headless | `unity run --command <Method> --log-file <path>` | `Unity.exe -quit -batchmode -executeMethod <Method> -logFile <path>` |
| Run affected PlayMode tests | `unity test --affected --log-file <path>` | `Unity.exe -quit -batchmode -runTests -testPlatform PlayMode ...` |
| Build Android APK | `unity build run` | `Unity.exe -quit -batchmode -executeMethod NAAD.Editor.NAADBuild.BuildAndroid` |
| Verify project structure | `unity run --command NAAD.Editor.Editor.VerifyProject` | `Unity.exe -quit -batchmode -executeMethod NAAD.Editor.Editor.VerifyProject` |

> **Migration note**: Replace `-- --headless` invocations with `unity run`. The Unity CLI handles batch mode internally. Do not pass `--headless` to `unity run`.

---

## Verification Commands (Modern Unity CLI)

### 1. Project Verification

Verify project structure, references, and configuration.

```bash
unity run --command NAAD.Editor.Editor.VerifyProject --log-file "Logs/ProjectVerification.log"
```

**Expected Output**: Success or detailed error report

### 2. Scene Verification

Verify all scenes are properly configured.

```bash
unity run --command NAAD.Editor.VerifyScenes --log-file "Logs/SceneVerification.log"
```

**Expected Output**: List of scenes with configuration status

### 3. Environment Verification

Verify environment scenes have required components.

```bash
unity run --command NAAD.Editor.VerifyEnvironmentScenes --log-file "Logs/EnvironmentVerification.log"
```

**Expected Output**: Environment scene verification report

### 4. Gate C Setup All Assets

Set up Gate C animation assets (rig, controller, mask, prefab).
**Menu**: `NAAD > Gate C > Setup All Assets`

```bash
unity run --command NAAD.Editor.SetupGateCAssets.RunFromCLI --log-file "Logs/GateCSetup.log"
```

**Expected Output**: Gate C assets created successfully

### 5. Verify Animation Assets

Verify Gate C animation contract (rig, avatar, clips, controller, prefab).
**Menu**: `NAAD > Verify Animation Assets`

```bash
unity run --command NAAD.Editor.VerifyAnimationAssets.RunVerification --log-file "Logs/AnimationVerification.log"
```

**Expected Output**: Animation asset verification report (JSON at `Logs/animation-verification-report.json`)

### 6. Player Navigation Verification

Verify player navigation in scenes.

```bash
unity run --command NAAD.Editor.VerifyPlayerNavigation --log-file "Logs/NavigationVerification.log"
```

**Expected Output**: Navigation verification report

### 7. UI Verification

Verify UI controllers and views are properly configured.

```bash
unity run --command NAAD.Editor.Editor.VerifyUI --log-file "Logs/UIVerification.log"
```

**Expected Output**: UI verification report

### 8. Service Verification

Verify networking services and wiring.

```bash
unity run --command NAAD.Editor.VerifyServices --log-file "Logs/ServiceVerification.log"
```

**Expected Output**: Service verification report

### 9. Unit Tests (Affected PlayMode)

Run affected PlayMode tests incrementally.

```bash
unity test --affected --log-file "Logs/UnitTests.log"
```

**Expected Output**: NUnit test report

### 10. Unit Tests (All PlayMode)

Run all PlayMode tests.

```bash
unity run --command NAAD.Editor.RunAllTests.RunPlayModeTests --log-file "Logs/UnitTests.log"
```

**Expected Output**: Test pass/fail summary in log

---

## Verification Commands (Legacy Editor Batch Mode)

Retained for compatibility. These use the traditional `-executeMethod` pattern.

### Legacy 1. Project Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.Editor.VerifyProject -logFile "Logs/ProjectVerification.log"
```

### Legacy 2. Scene Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyScenes -logFile "Logs/SceneVerification.log"
```

### Legacy 3. Environment Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyEnvironmentScenes -logFile "Logs/EnvironmentVerification.log"
```

### Legacy 4. Gate C Setup All Assets

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.SetupGateCAssets.RunFromCLI -logFile "Logs/GateCSetup.log"
```

### Legacy 5. Verify Animation Assets

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyAnimationAssets.RunVerification -logFile "Logs/AnimationVerification.log"
```

### Legacy 6. Player Navigation Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyPlayerNavigation -logFile "Logs/NavigationVerification.log"
```

### Legacy 7. UI Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.Editor.VerifyUI -logFile "Logs/UIVerification.log"
```

### Legacy 8. Service Verification

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyServices -logFile "Logs/ServiceVerification.log"
```

### Legacy 9. Unit Tests (All PlayMode)

```bash
Unity.exe -quit -batchmode -projectPath "c:\NAAD\naija-after-dark\game\unity" -runTests -testPlatform PlayMode -testResults "Logs\TestResults.xml" -logFile "Logs\UnitTests.log"
```

---

## Batch Verification Script (Modern Unity CLI)

```batch
@echo off
set PROJECT=c:\NAAD\naija-after-dark\game\unity

echo 1. Project Verification
unity run --command NAAD.Editor.Editor.VerifyProject --project-path "%PROJECT%" --log-file "Logs\ProjectVerification.log"

echo 2. Scene Verification
unity run --command NAAD.Editor.VerifyScenes --project-path "%PROJECT%" --log-file "Logs\SceneVerification.log"

echo 3. Environment Verification
unity run --command NAAD.Editor.VerifyEnvironmentScenes --project-path "%PROJECT%" --log-file "Logs\EnvironmentVerification.log"

echo 4. Gate C Setup All Assets
unity run --command NAAD.Editor.SetupGateCAssets.RunFromCLI --project-path "%PROJECT%" --log-file "Logs\GateCSetup.log"

echo 5. Verify Animation Assets
unity run --command NAAD.Editor.VerifyAnimationAssets.RunVerification --project-path "%PROJECT%" --log-file "Logs\AnimationVerification.log"

echo 6. Navigation Verification
unity run --command NAAD.Editor.VerifyPlayerNavigation --project-path "%PROJECT%" --log-file "Logs\NavigationVerification.log"

echo 7. UI Verification
unity run --command NAAD.Editor.Editor.VerifyUI --project-path "%PROJECT%" --log-file "Logs\UIVerification.log"

echo 8. Service Verification
unity run --command NAAD.Editor.VerifyServices --project-path "%PROJECT%" --log-file "Logs\ServiceVerification.log"

echo 9. Unit Tests (Affected PlayMode)
unity test --affected --project-path "%PROJECT%" --log-file "Logs\UnitTests.log"

echo Verification Complete
pause
```

---

## CI/CD Integration

### GitHub Actions Example (Modern Unity CLI)

Create `.github/workflows/unity-verify.yml`:

```yaml
name: Unity Verification

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  verify:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Cache Unity Library
      uses: actions/cache@v3
      with:
        path: Library
        key: Library-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
        restore-keys: |
          Library-

    - name: Run Unity Verification
      run: |
        unity run --command NAAD.Editor.Editor.VerifyProject --log-file "Logs/ProjectVerification.log"
        unity run --command NAAD.Editor.VerifyScenes --log-file "Logs/SceneVerification.log"
        unity run --command NAAD.Editor.VerifyEnvironmentScenes --log-file "Logs/EnvironmentVerification.log"
        unity run --command NAAD.Editor.SetupGateCAssets.RunFromCLI --log-file "Logs/GateCSetup.log"
        unity run --command NAAD.Editor.VerifyAnimationAssets.RunVerification --log-file "Logs/AnimationVerification.log"
        unity run --command NAAD.Editor.VerifyPlayerNavigation --log-file "Logs/NavigationVerification.log"
        unity run --command NAAD.Editor.Editor.VerifyUI --log-file "Logs/UIVerification.log"
        unity run --command NAAD.Editor.VerifyServices --log-file "Logs/ServiceVerification.log"
        unity test --affected --log-file "Logs/UnitTests.log"
      env:
        UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
        UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}

    - name: Upload Logs
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: Verification Logs
        path: Logs/
```

### GitHub Actions Example (Legacy Editor Batch Mode)

```yaml
- name: Run Unity Verification (Legacy)
  uses: game-ci/unity-runner@v2
  env:
    UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
    UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
    UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
  with:
    targetPlatform: Android
    projectPath: .
    buildName: NAAD
    buildsPath: Build
    buildMethod: NAAD.Editor.Editor.VerifyProject
    version: 6000.6.5f1
    allowDirtyBuild: true
```

---

## Output Formats and Exit Codes

The Unity CLI supports multiple output formats:

| Format | Description |
|--------|-------------|
| `plain` | Human-readable text (default for terminal) |
| `tsv` | Tab-separated values (default for piped output) |
| `json` | JSON objects, one per line |
| `ndjson` | Newline-delimited JSON |

**Exit codes**:
- `0` — Success
- `1` — Error
- `130` — User cancellation

CI pipelines should branch on exit code, not on stdout content.

---

## Log Analysis

All commands generate log files in the `Logs/` directory. Review these files for:

- Errors and warnings
- Build times
- Asset import issues
- Test failures
- Performance metrics

Machine-readable reports are also written to `Logs/verification-report.json` and `Logs/animation-verification-report.json`.

## Troubleshooting

### Unity CLI Not Found

Ensure Unity CLI is in your PATH or use the full path:

```bash
# Modern CLI
unity --version

# Legacy Editor (full path)
"C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -quit -batchmode ...
```

### License Issues

Activate Unity license:

```bash
unity license return       # return existing
unity license activate <license-file>  # activate new
```

Or legacy:
```bash
Unity.exe -quit -batchmode -logFile - -manualLicenseFile <path-to-license-file>
```

### Build Failures

Check the build log for specific errors. Common issues:
- Missing scenes in Build Settings
- Android SDK not configured
- Texture compression errors
- Script compilation errors

### Test Failures

Check the test results XML file for specific test failures. Common issues:
- Missing test fixtures
- Service not initialized
- Scene not loaded

### Batch Mode Hangs

If a batch-mode Unity process never exits, a previous instance may hold a lock on the Library folder. Kill lingering Unity processes and wait before re-running.

## Next Steps

1. Implement required Editor methods (see "Implementation Required" notes above)
2. Test each command individually
3. Create batch script for verification
4. Integrate with CI/CD
5. Monitor logs for issues
6. Fix any verification failures

---

## Cloud Code Module Integration

**References**: `CloudCodeModules/docs/CloudCode_Modules.md` · `CloudCodeModules/`

### Module Project

| Component | Purpose |
|-----------|---------|
| `CloudCodeModules/NNPCCommunication_MODULE.csproj` | .NET 9 C# module project SDK |
| `CloudCodeModules/README.md` | Module structure, workflow, and CI/CD guide |

### Unity CLI Cloud Commands

```bash
# Start local Cloud Code server
unity cloud-code server start --module NNPCCommunication

# Deploy module
unity cloud-code deploy NNPCCommunication --project <project-id>

# Check deployment status
unity cloud-code status NNPCCommunication
```

### Cross-App Workflows

Below is the documented cross-app workflow for deploying Cloud Code modules in CI.

**Prerequisites:**
- `Unity CLI` installed
- `NAAD.CloudCode` module project scaffolded in `game/unity/Assets/NAAD/CloudCodeModules/`
- Cloud Code project created in Unity Cloud Code dashboard

**Workflow:**
1. Edit module C# code
2. Run `unity cloud-code server start` for local iteration
3. Run `unity cloud-code deploy NNPCCommunication` to deploy to Cloud Code
4. Generate type-safe client bindings: `unity cloud-code generate-bindings NNPCCommunication`
5. Import bindings into Unity client (SampleClient project)
6. Verify module functions via `unity test --affected --test-filter "CloudCodeTests"`

**Example module function:**

```csharp
[CloudCodeModule]
public class NpcCommunicationModule : INpcCommunicationModule
{
    [CloudCodeFunction("TalkToTunde")]
    public async Task<TalkToTundeResponse> TalkToTundeAsync(
        string playerMessage, string requestId)
    {
        // Validate, propose, then return validated response
        var props = await GetContextAsync(playerMessage, requestId);
        var ai = await GeminiClient.ProposeAsync(props, new CancellationToken());
        return ValidateAndClamp(ai, requestId);
    }
}
```