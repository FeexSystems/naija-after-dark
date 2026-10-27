using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NAAD.Gameplay;
using NAAD.Mock;
using NAAD.Networking;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that runs the NAAD automated test suite.
    /// <para>
    /// Executes MockCommandService, MockPlayerService, MockWorldStateService,
    /// MockPhoneService, and MockAuthenticationService tests and generates
    /// a JSON report. Invoked via the menu: <b>NAAD > Tests > Run All Tests</b>.
    /// </para>
    /// </summary>
    public static class AutomatedTestRunner
    {
        private const string ReportPath = "Logs/TestResults.json";

        [MenuItem("NAAD/Tests/Run All Tests", priority = 400)]
        public static void RunAllTests()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[TestRunner] The Unity Editor must be in Play Mode to run tests.");
                ExitBatch(1);
                return;
            }

            Debug.Log("[TestRunner] Starting automated test suite...");

            var tests = new List<TestResult>();

            var cmdTests = RunMockCommandTests();
            tests.AddRange(cmdTests);

            var playerTests = RunMockPlayerTests();
            tests.AddRange(playerTests);

            var worldTests = RunMockWorldTests();
            tests.AddRange(worldTests);

            var phoneTests = RunMockPhoneTests();
            tests.AddRange(phoneTests);

            var authTests = RunMockAuthTests();
            tests.AddRange(authTests);

            var sysTests = RunSystemIntegrationTests();
            tests.AddRange(sysTests);

            var passed = tests.FindAll(t => t.Success).Count;
            var failed = tests.FindAll(t => !t.Success).Count;

            var report = new
            {
                timestamp = System.DateTime.UtcNow.ToString("O"),
                total = tests.Count,
                passed = passed,
                failed = failed,
                tests = tests.ConvertAll(t => new { name = t.Name, success = t.Success, error = t.ErrorMessage })
            };

            var json = JsonUtility.ToJson(report, prettyPrint: true);
            File.WriteAllText(ReportPath, json);

            foreach (var t in tests)
            {
                Debug.Log($"[{t.Success ? "PASS" : "FAIL"}] {t.Name}{(string.IsNullOrEmpty(t.ErrorMessage) ? "" : $" - {t.ErrorMessage}")}");
            }
        private static List<TestResult> RunMockCommandTests()
        {
            var results = new List<TestResult>();

            var auth = new MockAuthenticationService();
            var players = MockPlayerService.GetOrCreate();
            var world = new MockWorldStateService();
            var phone = new MockPhoneService();
            var commands = new MockCommandService(auth, players, world, phone);

            var request = new CommandRequest
            {
                RequestId = "test-001",
                Command = new GameCommand
                {
                    Type = GameCommandType.Travel,
                    Payload = new Dictionary<string, object> { ["toLocationId"] = NightDistrictIds.SuyaSpot }
                },
                PlayerId = "mock_player"
            };

            var result = commands.ExecuteAsync(request).Result;
            results.Add(new TestResult
            {
                Name = "CommandService: Travel to Suya Spot",
                Success = result.Success,
                ErrorMessage = result.ErrorMessage
            });

            results.Add(new TestResult
            {
                Name = "CommandService: PendingTravel stored",
                Success = players.CurrentLocation == NightDistrictIds.SuyaSpot,
                ErrorMessage = $"Expected SuyaSpot, got {players.CurrentLocation}"
            });

            return results;
        }

        private static List<TestResult> RunMockPlayerTests()
        {
            var results = new List<TestResult>();
            var players = MockPlayerService.GetOrCreate();

            results.Add(new TestResult
            {
                Name = "PlayerService: Initial balance",
                Success = players.CurrentBalance == 50000,
                ErrorMessage = $"Expected 50000, got {players.CurrentBalance}"
            });

            players.TravelAsync(NightDistrictIds.Street, "test-002");
            var afterTravel = players.CurrentLocation == NightDistrictIds.CityStreet;
            results.Add(new TestResult
            {
                Name = "PlayerService: Travel updates location",
                Success = afterTravel,
                ErrorMessage = $"Expected CityStreet, got {players.CurrentLocation}"
            });

            players.UpdateBalanceAsync(40000, "test-004");
            results.Add(new TestResult
            {
                Name = "PlayerService: UpdateBalance",
                Success = players.CurrentBalance == 40000,
                ErrorMessage = $"Expected 40000, got {players.CurrentBalance}"
            });

            players.SetCareer("DJ");
            results.Add(new TestResult
            {
                Name = "PlayerService: SetCareer",
                Success = players.CurrentCareer == "DJ",
                ErrorMessage = $"Expected DJ, got {players.CurrentCareer}"
            });

            return results;
        }

        private static List<TestResult> RunMockWorldTests()
        {
            var results = new List<TestResult>();
            var world = new MockWorldStateService();

            results.Add(new TestResult
            {
                Name = "WorldStateService: Empty initial travels",
                Success = world.PendingTravels.Count == 0,
                ErrorMessage = $"Expected 0, got {world.PendingTravels.Count}"
            });

            world.TravelAsync("Apartment", "SuyaSpot", "test-001");
            results.Add(new TestResult
            {
                Name = "WorldStateService: Add pending travel",
                Success = world.PendingTravels.Count == 1,
                ErrorMessage = $"Expected 1, got {world.PendingTravels.Count}"
            });

            world.ApplyRelationshipDelta(NightDistrictIds.Tunde, 10);
            var tundeTrust = world.Relationships.Find(r => r.NpcId == NightDistrictIds.Tunde)?.Trust;
            results.Add(new TestResult
            {
                Name = "WorldStateService: Relationship delta applied",
                Success = tundeTrust == 60,
                ErrorMessage = $"Expected 60, got {tundeTrust}"
            });

            world.CommitMemory(NightDistrictIds.Tunde, "INTRO", "Player first visit to Beach");
            results.Add(new TestResult
            {
                Name = "WorldStateService: Commit memory",
                Success = world.Memories.Count == 1,
                ErrorMessage = $"Expected 1 memory, got {world.Memories.Count}"
            });

            return results;
        }

        private static List<TestResult> RunMockPhoneTests()
        {
            var results = new List<TestResult>();
            var phone = new MockPhoneService();

            results.Add(new TestResult
            {
                Name = "PhoneService: Seeded messages",
                Success = phone.Messages.Count >= 2,
                ErrorMessage = $"Expected at least 2 messages, got {phone.Messages.Count}"
            });

            phone.RespondToMessageAsync("msg_tunde_beach_1", "GO", "test-001");
            var responses = phone.Messages.Find(m => m.Id == "msg_tunde_beach_1");
            var responded = responses != null && responses.ChosenAction == "GO";
            results.Add(new TestResult
            {
                Name = "PhoneService: Respond to message",
                Success = responded,
                ErrorMessage = "Message not responded to"
            });

            phone.CompleteNightAsync();
            results.Add(new TestResult
            {
                Name = "PhoneService: Complete night",
                Success = phone.Messages.Count > 0,
                ErrorMessage = "Expected messages after complete"
            });

            return results;
        }

        private static List<TestResult> RunMockAuthTests()
        {
            var results = new List<TestResult>();
            var auth = new MockAuthenticationService();

            results.Add(new TestResult
            {
                Name = "AuthService: Not authenticated initially",
                Success = !auth.IsAuthenticated,
                ErrorMessage = "Expected not authenticated"
            });

            auth.SignInAsync("test@example.com", "password123");
            results.Add(new TestResult
            {
                Name = "AuthService: Authenticated after sign-in",
                Success = auth.IsAuthenticated,
                ErrorMessage = "Expected authenticated"
            });

            auth.SignOutAsync();
            results.Add(new TestResult
            {
                Name = "AuthService: Not authenticated after sign-out",
                Success = !auth.IsAuthenticated,
                ErrorMessage = "Expected not authenticated"
            });

            auth.RestoreSessionAsync();
            results.Add(new TestResult
            {
                Name = "AuthService: Restored after restore",
                Success = auth.IsAuthenticated,
                ErrorMessage = "Expected authenticated after restore"
            });

            return results;

        private static List<TestResult> RunSystemIntegrationTests()
        {
            var results = new List<TestResult>();
            var commands = new MockCommandService(
                new MockAuthenticationService(),
                MockPlayerService.GetOrCreate(),
                new MockWorldStateService(),
                new MockPhoneService());

            var r1 = commands.ExecuteAsync(new CommandRequest
            {
                RequestId = "flow-1",
                Command = new GameCommand { Type = GameCommandType.Travel, Payload = new Dictionary<string, object> { ["toLocationId"] = NightDistrictIds.SuyaSpot } }
            }).Result;

            results.Add(new TestResult
            {
                Name = "Integration: Travel to Suya Spot",
                Success = r1.Success,
                ErrorMessage = r1.ErrorMessage
            });

            var r2 = commands.ExecuteAsync(new CommandRequest
            {
                RequestId = "flow-2",
                Command = new GameCommand { Type = GameCommandType.Spend, Payload = new Dictionary<string, object> { ["sku"] = "SUYA_PLATE" } }
            }).Result;

            results.Add(new TestResult
            {
                Name = "Integration: Buy SUYA_PLATE",
                Success = r2.Success,
                ErrorMessage = r2.ErrorMessage
            });

            var r3 = commands.ExecuteAsync(new CommandRequest
            {
                RequestId = "flow-3",
                Command = new GameCommand { Type = GameCommandType.Travel, Payload = new Dictionary<string, object> { ["toLocationId"] = NightDistrictIds.Nightclub } }
            }).Result;

            results.Add(new TestResult
            {
                Name = "Integration: Travel to Nightclub",
                Success = r3.Success,
                ErrorMessage = r3.ErrorMessage
            });

            var r4 = commands.ExecuteAsync(new CommandRequest
            {
                RequestId = "flow-4",
                Command = new GameCommand { Type = GameCommandType.Spend, Payload = new Dictionary<string, object> { ["sku"] = "CLUB_TICKET" } }
            }).Result;

            results.Add(new TestResult
            {
                Name = "Integration: Buy CLUB_TICKET",
                Success = r4.Success,
                ErrorMessage = r4.ErrorMessage
            });

            return results;
        }

        [Serializable]
        public class TestResult
        {
            public string Name;
            public bool Success;
            public string ErrorMessage = "";
        }
    }
}
        }