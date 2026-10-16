# NAAD Unity CLI Verification Commands

**Generated**: 2026-10-13  
**Purpose**: Automated verification commands for Unity CLI

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
c:\Users\user\naija-after-dark-1\game\unity
```

Adjust the `-projectPath` parameter if your project is located elsewhere.

## Verification Commands

### 1. Project Verification

Verify project structure, references, and configuration.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.VerifyProject -logFile "Logs/ProjectVerification.log"
```

**Expected Output**: Success or detailed error report

**Implementation Required**: Create `NAAD.Editor.Editor.VerifyProject()` static method

---

### 2. Scene Verification

Verify all scenes are properly configured.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyScenes -logFile "Logs/SceneVerification.log"
```

**Expected Output**: List of scenes with configuration status

**Implementation Required**: Create `NAAD.Editor.VerifyScenes()` static method

---

### 3. Environment Verification

Verify environment scenes have required components.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyEnvironmentScenes -logFile "Logs/EnvironmentVerification.log"
```

**Expected Output**: Environment scene verification report

**Implementation Required**: Already exists (`VerifyEnvironmentScenes.cs`)

---

### 4. Animation Verification

Verify animation assets are present and configured.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyAnimationAssets -logFile "Logs/AnimationVerification.log"
```

**Expected Output**: Animation asset verification report

**Implementation Required**: Already exists (`VerifyAnimationAssets.cs`)

---

### 5. Player Navigation Verification

Verify player navigation in scenes.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyPlayerNavigation -logFile "Logs/NavigationVerification.log"
```

**Expected Output**: Navigation verification report

**Implementation Required**: Already exists (`VerifyPlayerNavigation.cs`)

---

### 6. UI Verification

Verify UI controllers and views are properly configured.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.VerifyUI -logFile "Logs/UIVerification.log"
```

**Expected Output**: UI verification report

**Implementation Required**: Create `NAAD.Editor.Editor.VerifyUI()` static method

---

### 7. Service Verification

Verify service configuration and connectivity.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyServices -logFile "Logs/ServiceVerification.log"
```

**Expected Output**: Service connectivity report

**Implementation Required**: Create `NAAD.Editor.VerifyServices()` static method

---

### 8. Run Unit Tests

Execute all unit tests in the project.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -runTests -testPlatform PlayMode -testResults "Logs/TestResults.xml" -logFile "Logs/UnitTests.log"
```

**Expected Output**: Test results in XML format

---

### 9. Run Edit Mode Tests

Execute edit mode tests.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -runTests -testPlatform EditMode -testResults "Logs/EditModeTestResults.xml" -logFile "Logs/EditModeTests.log"
```

**Expected Output**: Test results in XML format

---

### 10. First Night Smoke Test

Run the First Night smoke test.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Gameplay.FirstNight.FirstNightSmokeTest.RunSmokeTest -logFile "Logs/FirstNightSmokeTest.log"
```

**Expected Output**: Smoke test report with step-by-step results

**Implementation Required**: Already exists (`FirstNightSmokeTest.cs`)

---

## Build Commands

### 11. Build Android APK

Build Android APK for release.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildAndroid -logFile "Logs/AndroidBuild.log"
```

**Expected Output**: APK file in build output directory

**Implementation Required**: Create `NAAD.Editor.BuildAndroid()` static method

---

### 12. Build Android Development Build

Build Android APK for development (with debugging).

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildAndroidDevelopment -logFile "Logs/AndroidDevBuild.log"
```

**Expected Output**: Development APK with debugging enabled

**Implementation Required**: Create `NAAD.Editor.BuildAndroidDevelopment()` static method

---

### 13. Build Android AAB

Build Android App Bundle for Play Store.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildAndroidAAB -logFile "Logs/AndroidAABBuild.log"
```

**Expected Output**: AAB file in build output directory

**Implementation Required**: Create `NAAD.Editor.BuildAndroidAAB()` static method

---

## Asset Import Commands

### 14. Reimport All Assets

Force reimport of all assets (useful after configuration changes).

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod UnityEditor.AssetDatabase.Refresh -logFile "Logs/AssetRefresh.log"
```

**Expected Output**: All assets reimported

---

### 15. Reimport Textures

Reimport all textures with new compression settings.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.ReimportTextures -logFile "Logs/TextureReimport.log"
```

**Expected Output**: All textures reimported with new settings

**Implementation Required**: Create `NAAD.Editor.Editor.ReimportTextures()` static method

---

## Package Management Commands

### 16. List Installed Packages

List all installed Unity packages.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.ListPackages -logFile "Logs/PackageList.log"
```

**Expected Output**: List of installed packages

**Implementation Required**: Create `NAAD.Editor.Editor.ListPackages()` static method

---

### 17. Add Package

Add a Unity package by name or Git URL.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.AddPackage -package "com.unity.textmeshpro" -logFile "Logs/AddPackage.log"
```

**Expected Output**: Package installed

**Implementation Required**: Create `NAAD.Editor.Editor.AddPackage()` static method

---

## Performance Profiling Commands

### 18. Profile Build

Build with profiling enabled.

```bash
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildAndroidWithProfiling -logFile "Logs/AndroidProfilingBuild.log"
```

**Expected Output**: APK with profiling enabled

**Implementation Required**: Create `NAAD.Editor.BuildAndroidWithProfiling()` static method

---

## Required Editor Methods

### NAAD.Editor.Editor.VerifyProject()

```csharp
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static class VerifyProject
    {
        public static void Execute()
        {
            Debug.Log("Verifying NAAD project...");
            
            // Check project structure
            // Check required folders
            // Check required scripts
            // Check configuration files
            // Generate report
            
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.VerifyScenes()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public static class VerifyScenes
    {
        public static void Execute()
        {
            Debug.Log("Verifying scenes...");
            
            // Load each scene
            // Check required components
            // Check build settings
            // Generate report
            
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.Editor.VerifyUI()

```csharp
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static class VerifyUI
    {
        public static void Execute()
        {
            Debug.Log("Verifying UI...");
            
            // Check UI controllers
            // Check UI prefabs
            // Check data contracts
            // Generate report
            
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.VerifyServices()

```csharp
using UnityEditor;
using UnityEngine;
using NAAD.Core.Bootstrap;

namespace NAAD.Editor
{
    public static class VerifyServices
    {
        public static void Execute()
        {
            Debug.Log("Verifying services...");
            
            // Check SupabaseConfig
            // Check service wiring
            // Test service connectivity
            // Generate report
            
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.BuildAndroid()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace NAAD.Editor
{
    public static class BuildAndroid
    {
        public static void Execute()
        {
            Debug.Log("Building Android APK...");
            
            string[] scenes = { "Assets/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD.apk";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };
            
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.totalErrors} errors");
            }
            
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
```

### NAAD.Editor.BuildAndroidDevelopment()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace NAAD.Editor
{
    public static class BuildAndroidDevelopment
    {
        public static void Execute()
        {
            Debug.Log("Building Android Development APK...");
            
            string[] scenes = { "Assets/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD-Dev.apk";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Development build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"Development build failed: {report.summary.totalErrors} errors");
            }
            
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
```

### NAAD.Editor.BuildAndroidAAB()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Android;

namespace NAAD.Editor
{
    public static class BuildAndroidAAB
    {
        public static void Execute()
        {
            Debug.Log("Building Android AAB...");
            
            string[] scenes = { "Assets/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD.aab";
            
            EditorUserBuildSettings.buildAppBundle = true;
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };
            
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"AAB build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"AAB build failed: {report.summary.totalErrors} errors");
            }
            
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
```

### NAAD.Editor.Editor.ReimportTextures()

```csharp
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static class ReimportTextures
    {
        public static void Execute()
        {
            Debug.Log("Reimporting textures...");
            
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
            
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            
            Debug.Log($"Reimported {textureGuids.Length} textures");
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.Editor.ListPackages()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

namespace NAAD.Editor
{
    public static class ListPackages
    {
        public static void Execute()
        {
            Debug.Log("Listing installed packages...");
            
            var listRequest = Client.List();
            
            while (!listRequest.IsCompleted)
            {
                System.Threading.Thread.Sleep(100);
            }
            
            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    Debug.Log($"{package.name} - {package.version}");
                }
            }
            else
            {
                Debug.LogError($"Failed to list packages: {listRequest.Error.message}");
            }
            
            EditorApplication.Exit(0);
        }
    }
}
```

### NAAD.Editor.Editor.AddPackage()

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

namespace NAAD.Editor
{
    public static class AddPackage
    {
        public static string packageName = "";
        
        public static void Execute()
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Package name not specified");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log($"Adding package: {packageName}");
            
            var addRequest = Client.Add(packageName);
            
            while (!addRequest.IsCompleted)
            {
                System.Threading.Thread.Sleep(100);
            }
            
            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Package added: {addRequest.Result.name}");
            }
            else
            {
                Debug.LogError($"Failed to add package: {addRequest.Error.message}");
            }
            
            EditorApplication.Exit(addRequest.Status == StatusCode.Success ? 0 : 1);
        }
    }
}
```

## Batch Script for Windows

Create `verify.bat` in the project root:

```batch
@echo off
echo NAAD Verification Script
echo ========================

echo 1. Project Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.VerifyProject -logFile "Logs\ProjectVerification.log"

echo 2. Scene Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyScenes -logFile "Logs\SceneVerification.log"

echo 3. Environment Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyEnvironmentScenes -logFile "Logs\EnvironmentVerification.log"

echo 4. Animation Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyAnimationAssets -logFile "Logs\AnimationVerification.log"

echo 5. Navigation Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyPlayerNavigation -logFile "Logs\NavigationVerification.log"

echo 6. UI Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.Editor.VerifyUI -logFile "Logs\UIVerification.log"

echo 7. Service Verification
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -executeMethod NAAD.Editor.VerifyServices -logFile "Logs\ServiceVerification.log"

echo 8. Unit Tests
unity -quit -batchmode -projectPath "c:\Users\user\naija-after-dark-1\game\unity" -runTests -testPlatform PlayMode -testResults "Logs\TestResults.xml" -logFile "Logs\UnitTests.log"

echo Verification Complete
pause
```

## CI/CD Integration

### GitHub Actions Example

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
    
    - name: Upload Logs
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: Verification Logs
        path: Logs/
```

## Log Analysis

All commands generate log files in the `Logs/` directory. Review these files for:

- Errors and warnings
- Build times
- Asset import issues
- Test failures
- Performance metrics

## Troubleshooting

### Unity CLI Not Found

Ensure Unity CLI is in your PATH or use the full path:

```bash
"C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -quit -batchmode ...
```

### License Issues

Activate Unity license:

```bash
unity -quit -batchmode -logFile - -manualLicenseFile <path-to-license-file>
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

## Next Steps

1. Implement required Editor methods
2. Test each command individually
3. Create batch script for verification
4. Integrate with CI/CD
5. Monitor logs for issues
6. Fix any verification failures
