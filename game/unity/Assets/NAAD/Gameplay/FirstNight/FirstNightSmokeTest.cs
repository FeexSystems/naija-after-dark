using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.Gameplay.FirstNight
{
    /// <summary>
    /// Smoke test for the First Night gameplay loop.
    /// Runs the complete sequence and reports the first failed step with evidence.
    /// </summary>
    public class FirstNightSmokeTest : MonoBehaviour
    {
        [Header("Smoke Test Configuration")]
        [SerializeField] private bool autoRunOnStart = false;
        [SerializeField] private string phoneAction = "GO";
        [SerializeField] private bool includeClubTicket = true;

        private FirstNightController _controller;
        private List<SmokeTestResult> _results = new List<SmokeTestResult>();

        private void Start()
        {
            if (autoRunOnStart)
            {
                RunSmokeTestAsync();
            }
        }

        public async void RunSmokeTestAsync()
        {
            Debug.Log("[SmokeTest] Starting First Night smoke test...");

            _controller = GetComponent<FirstNightController>();
            if (_controller == null)
            {
                _controller = gameObject.AddComponent<FirstNightController>();
            }

            _controller.IncludeClubTicket = includeClubTicket;

            _results.Clear();

            // Run full sequence
            await TestStep("EnterApartment", async () => await _controller.EnterApartmentAsync());
            if (LastStepFailed()) return;

            await TestStep("StartNight", async () => await _controller.StartNightAsync());
            if (LastStepFailed()) return;

            await TestStep("RespondToInvite", async () => await _controller.RespondToInviteAsync(phoneAction));
            if (LastStepFailed()) return;

            await TestStep("SelectDestination", async () => await _controller.SelectDestinationAsync(NightDistrictIds.SuyaSpot));
            if (LastStepFailed()) return;

            await TestStep("TravelToSuya", async () => await _controller.TravelToAsync(NightDistrictIds.SuyaSpot));
            if (LastStepFailed()) return;

            await TestStep("GoToSuyaAndBuy", async () => await _controller.GoToSuyaAndBuyAsync());
            if (LastStepFailed()) return;

            if (includeClubTicket)
            {
                await TestStep("GoToClubAndBuyTicket", async () => await _controller.GoToClubAndBuyTicketAsync());
                if (LastStepFailed()) return;
            }

            await TestStep("GoToBeach", async () => await _controller.GoToBeachAsync());
            if (LastStepFailed()) return;

            await TestStep("ReturnHome", async () => await _controller.ReturnHomeAsync());
            if (LastStepFailed()) return;

            await TestStep("CompleteNight", async () => await _controller.CompleteNightAsync());

            // Report results
            ReportResults();
        }

        private async Task TestStep(string stepName, Func<Task<bool>> action)
        {
            Debug.Log($"[SmokeTest] Testing: {stepName}");
            var result = new SmokeTestResult
            {
                StepName = stepName,
                StartTime = DateTime.Now
            };

            try
            {
                var success = await action();
                result.Success = success;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;

                if (!success)
                {
                    result.ErrorMessage = _controller.LastError ?? "Unknown error";
                    result.Step = _controller.Step;
                    Debug.LogError($"[SmokeTest] FAILED: {stepName} - {result.ErrorMessage}");
                }
                else
                {
                    Debug.Log($"[SmokeTest] PASSED: {stepName} ({result.Duration.TotalSeconds:F2}s)");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                Debug.LogError($"[SmokeTest] EXCEPTION: {stepName} - {ex.Message}");
            }

            _results.Add(result);
        }

        private bool LastStepFailed()
        {
            if (_results.Count == 0) return false;
            var lastResult = _results[_results.Count - 1];
            return !lastResult.Success;
        }

        private void ReportResults()
        {
            var passed = _results.FindAll(r => r.Success).Count;
            var failed = _results.FindAll(r => !r.Success).Count;
            var total = _results.Count;

            Debug.Log($"[SmokeTest] Smoke Test Complete: {passed}/{total} passed, {failed} failed");

            if (failed > 0)
            {
                // SmokeTestResult is a struct, so List.Find returns default (never null)
                // when nothing matches. Use FindIndex (-1 when absent) to detect a real
                // failure before dereferencing.
                var firstFailureIndex = _results.FindIndex(r => !r.Success);
                if (firstFailureIndex >= 0)
                {
                    var firstFailure = _results[firstFailureIndex];
                    Debug.LogError($"[SmokeTest] FIRST FAILURE: {firstFailure.StepName}");
                    Debug.LogError($"[SmokeTest] Error: {firstFailure.ErrorMessage}");
                    Debug.LogError($"[SmokeTest] Step: {firstFailure.Step}");
                    Debug.LogError($"[SmokeTest] Duration: {firstFailure.Duration.TotalSeconds:F2}s");
                    if (firstFailure.Exception != null)
                    {
                        Debug.LogError($"[SmokeTest] Exception: {firstFailure.Exception.StackTrace}");
                    }
                }
            }
            else
            {
                Debug.Log("[SmokeTest] ✓ All steps passed successfully");
            }

            // Generate report file
            GenerateReportFile();
        }

        private void GenerateReportFile()
        {
            var reportPath = "Assets/NAAD/Gameplay/FirstNight/SMOKE_TEST_REPORT.txt";
            var report = new System.Text.StringBuilder();

            report.AppendLine("NAAD First Night Smoke Test Report");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Summary: {_results.FindAll(r => r.Success).Count}/{_results.Count} steps passed");

            foreach (var result in _results)
            {
                report.AppendLine();
                report.AppendLine($"Step: {result.StepName}");
                report.AppendLine($"Status: {(result.Success ? "PASSED" : "FAILED")}");
                report.AppendLine($"Duration: {result.Duration.TotalSeconds:F2}s");
                report.AppendLine($"Step State: {result.Step}");

                if (!result.Success)
                {
                    report.AppendLine($"Error: {result.ErrorMessage}");
                    if (result.Exception != null)
                    {
                        report.AppendLine($"Exception: {result.Exception.Message}");
                        report.AppendLine($"StackTrace: {result.Exception.StackTrace}");
                    }
                }
            }

            System.IO.File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[SmokeTest] Report saved to: {reportPath}");
        }

        private struct SmokeTestResult
        {
            public string StepName;
            public bool Success;
            public DateTime StartTime;
            public DateTime EndTime;
            public TimeSpan Duration;
            public string ErrorMessage;
            public FirstNightStep Step;
            public Exception Exception;
        }
    }
}
