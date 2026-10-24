@echo off
echo NAAD Verification Script
echo ========================

echo 1. Project Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyProject.Execute -logFile "Logs\ProjectVerification.log"
if %errorlevel% neq 0 (
    echo Project Verification FAILED
    exit /b 1
)

echo 2. Scene Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyScenes.Execute -logFile "Logs\SceneVerification.log"
if %errorlevel% neq 0 (
    echo Scene Verification FAILED
    exit /b 1
)

echo 3. Environment Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyEnvironmentScenes.RunVerification -logFile "Logs\EnvironmentVerification.log"
if %errorlevel% neq 0 (
    echo Environment Verification FAILED
    exit /b 1
)

echo 4. Animation Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyAnimationAssets.RunVerification -logFile "Logs\AnimationVerification.log"
if %errorlevel% neq 0 (
    echo Animation Verification FAILED
    exit /b 1
)

echo 5. Navigation Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyPlayerNavigation.RunVerification -logFile "Logs\NavigationVerification.log"
if %errorlevel% neq 0 (
    echo Navigation Verification FAILED
    exit /b 1
)

echo 6. UI Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyUI.Execute -logFile "Logs\UIVerification.log"
if %errorlevel% neq 0 (
    echo UI Verification FAILED
    exit /b 1
)

echo 7. Service Verification
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyServices.Execute -logFile "Logs\ServiceVerification.log"
if %errorlevel% neq 0 (
    echo Service Verification FAILED
    exit /b 1
)

echo 8. Unit Tests
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -runTests -testPlatform PlayMode -testResults "Logs\TestResults.xml" -logFile "Logs\UnitTests.log"
if %errorlevel% neq 0 (
    echo Unit Tests FAILED
    exit /b 1
)

echo Verification Complete
exit /b 0
