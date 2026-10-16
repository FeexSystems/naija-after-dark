using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Shared plumbing for NAAD CLI verification and build entry points.
    ///
    /// The documented commands in Assets/NAAD/UNITY_CLI_COMMANDS.md were written as a static
    /// Execute() per class. The real Unity CLI contract is:

    unity -quit -batchmode -projectPath <path> -executeMethod NAAD.Editor.Editor.VerifyProject ...

    Unity resolves -executeMethod as "TypeName.MethodName". Because this file declares
    namespace NAAD.Editor AND class Editor, methods declared on this partial class use
    the fully qualified type name NAAD.Editor.Editor. Other Editor utility classes in
    this namespace keep their own type names, e.g. NAAD.Editor.NAADBuild.Verify and
    NAAD.Editor.FirstNightLiveTest.Run.

    Exit codes are meaningful for CI: 0 = pass, non-zero = fail. Errors are always written to
    /// the Unity log (which -logFile captures) and summarised in the machine-readable JSON report
    /// so the pipeline can fail the job on a real verification failure rather than on a log scrape.
    /// </summary>
    public static partial class Editor
    {
        /// <summary>Machine-readable report consumed by CI (see .github/workflows/unity-verify.yml).</summary>
        public const string ReportRelativePath = "Logs/verification-report.json";

        internal sealed class VerificationReport
        {
            public int Failed;
            public int Passed;
        }

        /// <summary>
        /// Writes the aggregate verification report and terminates the batch-mode editor with the
        /// matching exit code. Call this exactly once, as the last statement of a verification
        /// command. Without it a batch-mode run either hangs (no Exit) or returns a misleading 0.
        /// </summary>
        internal static void Finish(string commandName, VerificationReport report)
        {
            var total = report.Passed + report.Failed;
            var json = "{\n" +
                       $"  \"command\": \"{commandName}\",\n" +
                       $"  \"passed\": {report.Passed},\n" +
                       $"  \"failed\": {report.Failed},\n" +
                       $"  \"total\": {total},\n" +
                       $"  \"success\": {(report.Failed == 0 ? "true" : "false")},\n" +
                       $"  \"generatedUtc\": \"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"\n" +
                       "}\n";

            WriteReport(commandName, json);

            if (report.Failed == 0)
                Debug.Log($"[NAAD][Verify] {commandName}: PASS ({report.Passed}/{total} checks).");
            else
                Debug.LogError($"[NAAD][Verify] {commandName}: FAIL ({report.Failed} of {total} checks failed).");

            ExitBatch(report.Failed == 0 ? 0 : 1);
        }

        /// <summary>Logs a failed check and increments the failure counter.</summary>
        internal static void Fail(VerificationReport report, string message)
        {
            report.Failed++;
            Debug.LogError($"[NAAD][Verify] FAIL: {message}");
        }

        /// <summary>Logs a passed check and increments the pass counter.</summary>
        internal static void Pass(VerificationReport report, string message)
        {
            report.Passed++;
            Debug.Log($"[NAAD][Verify] PASS: {message}");
        }

        /// <summary>
        /// Exits the batch-mode editor. Guarded so an interactive/menu invocation is never killed.
        /// EditorApplication.Exit is required for CI: a batch-mode process that never calls it hangs
        /// until the runner's timeout, which is reported as an infrastructure failure, not a real one.
        /// </summary>
        internal static void ExitBatch(int exitCode)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            else if (exitCode != 0)
            {
                Debug.LogWarning($"[NAAD][Verify] Non-zero exit ({exitCode}) suppressed outside batch mode.");
            }
        }

        private static void WriteReport(string commandName, string json)
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot))
                {
                    Debug.LogWarning("[NAAD][Verify] Could not resolve project root; report not written.");
                    return;
                }

                var reportPath = Path.Combine(projectRoot, ReportRelativePath);
                var directory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(reportPath, json);
                Debug.Log($"[NAAD][Verify] Report written to {ReportRelativePath}");
            }
            catch (Exception ex)
            {
                // Reporting is best-effort; never let it mask the actual verification result.
                Debug.LogWarning($"[NAAD][Verify] Could not write report: {ex.Message}");
            }
        }
    }
}
