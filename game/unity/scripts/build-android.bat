@echo off
echo NAAD Android Build Script
echo ========================

if "%1"=="dev" (
    echo Building Android Development APK...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildMethods.BuildAndroidDevelopment -logFile "Logs\AndroidDevBuild.log"
) else if "%1"=="aab" (
    echo Building Android AAB for Play Store...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildMethods.BuildAndroidAAB -logFile "Logs\AndroidAABBuild.log"
) else if "%1"=="profile" (
    echo Building Android APK with profiling...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildMethods.BuildAndroidWithProfiling -logFile "Logs\AndroidProfilingBuild.log"
) else (
    echo Building Android Release APK...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildMethods.BuildAndroid -logFile "Logs\AndroidBuild.log"
)

if %errorlevel% neq 0 (
    echo Build FAILED
    exit /b 1
)

echo Build Complete
exit /b 0
