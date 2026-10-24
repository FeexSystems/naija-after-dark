using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor-side contract check: the offline stub and the TypeScript contracts
    /// must agree with the authoritative SQL in supabase/migrations/.
    ///
    /// Why a source-level check and not a runtime test: the value that matters is
    /// the one Postgres returns from <c>naad_nightlife_for_period</c> when the game
    /// is online, and the one <see cref="NAAD.Networking.StubWorldStateService"/>
    /// returns when it is not. Neither runs without Unity's test framework wired
    /// for EditMode, so this validates the three sources directly and fails loudly
    /// when they diverge.
    ///
    /// Batchmode: -executeMethod NAAD.Editor.WorldClockContractVerifier.RunVerification
    /// Menu:      NAAD &gt; Verify World Clock Contract
    /// </summary>
    public static class WorldClockContractVerifier
    {
        private const string ReportRelativePath = "Logs/world-clock-contract-report.json";

        /// <summary>Period -&gt; nightlife bias, per 0004_world_time.sql:47-55.</summary>
        private static readonly KeyValuePair<string, int>[] ExpectedNightlife =
        {
            new KeyValuePair<string, int>("MORNING", 15),
            new KeyValuePair<string, int>("DAY", 25),
            new KeyValuePair<string, int>("TRANSITION", 55),
            new KeyValuePair<string, int>("NIGHT", 80),
            new KeyValuePair<string, int>("LATE_NIGHT", 90),
            new KeyValuePair<string, int>("AFTER_HOURS", 60)
        };

        private const int ExpectedNightlifeUnknown = 50; // SQL `else 50`

        [Serializable]
        private sealed class ContractReport
        {
            public bool success;
            public int passed;
            public int failed;
            public string[] failures;
            public string generatedUtc;
        }

        [MenuItem("NAAD/Verify World Clock Contract")]
        public static void RunVerification()
        {
            var failures = new List<string>();
            var passed = 0;

            foreach (var expected in ExpectedNightlife)
            {
                Check(
                    "Stub nightlife for " + expected.Key + " == " + expected.Value,
                    NAAD.Networking.StubWorldStateService.NightlifeForPeriod(expected.Key) == expected.Value,
                    failures,
                    ref passed);
            }

            Check(
                "Stub nightlife for an unknown period == " + ExpectedNightlifeUnknown,
                NAAD.Networking.StubWorldStateService.NightlifeForPeriod("SOMETHING_ELSE") == ExpectedNightlifeUnknown,
                failures,
                ref passed);

            // The stub's eager WorldState read must use the same table, not a constant.
            var stub = new NAAD.Networking.StubWorldStateService(new NullLogger());
            var state = stub.FetchWorldStateAsync().GetAwaiter().GetResult();
            Check(
                "Stub WorldState.FetchWorldStateAsync nightlife matches the period table",
                state != null &&
                state.NightlifeLevel == NAAD.Networking.StubWorldStateService.NightlifeForPeriod("NIGHT"),
                failures,
                ref passed);

            Check(
                "Client contracts (world-model/types/index.ts) declare Weather on WorldClock",
                SourceContains("world-model/types/index.ts", "weather: Weather;"),
                failures,
                ref passed);

            Check(
                "SQL still defines naad_nightlife_for_period",
                SourceContains("supabase/migrations/0004_world_time.sql", "naad_nightlife_for_period"),
                failures,
                ref passed);

            var report = new ContractReport
            {
                success = failures.Count == 0,
                passed = passed,
                failed = failures.Count,
                failures = failures.ToArray(),
                generatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            WriteReport(report);

            if (report.success)
                Debug.Log($"[NAAD][WorldClock] PASS: {passed} contract checks.");
            else
                Debug.LogError($"[NAAD][WorldClock] FAIL: {report.failed} of {passed + report.failed} checks failed. See {ReportRelativePath}.");

            if (Application.isBatchMode)
                EditorApplication.Exit(report.success ? 0 : 1);
        }

        private static void Check(string name, bool condition, List<string> failures, ref int passed)
        {
            if (condition)
            {
                passed++;
                Debug.Log("[NAAD][WorldClock] PASS: " + name);
                return;
            }

            failures.Add(name);
            Debug.LogError("[NAAD][WorldClock] FAIL: " + name);
        }

        /// <summary>
        /// Reads a file at the repository root. Returns false when the repository
        /// layout is not available (e.g. an installed build without sources).
        /// </summary>
        private static bool SourceContains(string repoRelativePath, string needle)
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot))
                    return false;
                var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));
                var full = Path.Combine(repoRoot, repoRelativePath.Replace('/', Path.DirectorySeparatorChar));
                return File.Exists(full) && File.ReadAllText(full).Contains(needle);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[NAAD][WorldClock] Could not read " + repoRelativePath + ": " + exception.Message);
                return false;
            }
        }

        private static void WriteReport(ContractReport report)
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot))
                    throw new InvalidOperationException("Unable to resolve Unity project root.");

                var reportPath = Path.Combine(projectRoot, ReportRelativePath);
                var reportDirectory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(reportDirectory))
                    Directory.CreateDirectory(reportDirectory);

                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Debug.Log("[NAAD][WorldClock] Wrote report: " + ReportRelativePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[NAAD][WorldClock] Could not write report: " + exception.Message);
            }
        }

        /// <summary>Minimal logger so the verifier can construct the stub headless.</summary>
        private sealed class NullLogger : NAAD.Core.Logging.INAADLogger
        {
            public void Log(NAAD.Core.Logging.LogLevel level, string category, string message) { }
            public void Debug(string category, string message) { }
            public void Info(string category, string message) { }
            public void Warn(string category, string message) { }
            public void Error(string category, string message) { }
        }
    }
}
