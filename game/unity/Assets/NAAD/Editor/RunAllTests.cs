#if NAAD_LEGACY_EDITOR_TOOLS
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using System;

namespace NAAD.Editor
{
    /// <summary>
    /// Runs all EditMode and PlayMode tests.
    /// Run from Unity Editor menu: NAAD > Tests > Run All Tests
    /// </summary>
    public static class RunAllTests
    {
        [MenuItem("NAAD/Tests/Run All Tests")]
        public static void RunAllTestsMenu()
        {
            Debug.Log("[NAAD][Tests] Starting all tests...");
            RunTests();
        }

        [MenuItem("NAAD/Tests/Run EditMode Tests")]
        public static void RunEditModeTests()
        {
            Debug.Log("[NAAD][Tests] Running EditMode tests...");
            RunTests(TestMode.EditMode);
        }

        [MenuItem("NAAD/Tests/Run PlayMode Tests")]
        public static void RunPlayModeTests()
        {
            Debug.Log("[NAAD][Tests] Running PlayMode tests...");
            RunTests(TestMode.PlayMode);
        }

        private static void RunTests(TestMode mode = TestMode.EditMode | TestMode.PlayMode)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter { testMode = mode };
            var settings = new ExecutionSettings(filter);
            api.RegisterCallbacks(new TestCallbacks());
            api.Execute(settings);
        }

        private class TestCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { Debug.Log("[NAAD][Tests] Run started"); }
            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"[NAAD][Tests] Run finished: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped");
                if (result.FailCount > 0)
                    Debug.LogError("[NAAD][Tests] Some tests failed!");
            }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
#endif
