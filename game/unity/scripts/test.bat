@echo off
echo NAAD Test Script
echo ===============

if "%1"=="edit" (
    echo Running Edit Mode Tests...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -runTests -testPlatform EditMode -testResults "Logs\EditModeTestResults.xml" -logFile "Logs\EditModeTests.log"
) else if "%1"=="firstnight" (
    echo Running First Night Smoke Test...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.FirstNightLiveTest.Run -logFile "Logs\FirstNightSmokeTest.log" -manualExit
) else (
    echo Running Play Mode Tests...
    unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -runTests -testPlatform PlayMode -testResults "Logs\TestResults.xml" -logFile "Logs\UnitTests.log"
)

if %errorlevel% neq 0 (
    echo Tests FAILED
    exit /b 1
)

echo Tests Complete
exit /b 0
