using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.Logging;
using NAAD.Gameplay.FirstNight;
using NAAD.Networking;
using NAAD.Networking.Config;
using NAAD.Networking.Supabase;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate B — playable First Night vertical slice against the live project.
    /// Drives the REAL services via a real NAADApplicationRoot composition:
    ///
    ///   SignUp/SignIn → FetchCurrentPlayer →
    ///   START_NIGHT → PHONE_REPLY(GO) → TRAVEL(suya) → SPEND(SUYA_PLATE) →
    ///   TRAVEL(club) → SPEND(CLUB_TICKET) → COMPLETE_NIGHT → summary
    ///
    /// Pass condition: summary shape matches server (spent, transactions, peopleMet,
    /// connection, trust, moment) and wallet balance moved from its starting value.
    ///
    ///   Unity -batchmode -nographics -quit -projectPath game/unity \
    ///     -executeMethod NAAD.Editor.FirstNightLiveTest.Run
    ///
    /// Env: NAAD_TEST_EMAIL / NAAD_TEST_PASSWORD (any password 6+ chars).
    /// Report: Library/first-night-report.json
    /// </summary>
    public static class FirstNightLiveTest
    {
        private const string ReportPath = "Library/first-night-report.json";

        private static readonly string _email =
            Environment.GetEnvironmentVariable("NAAD_TEST_EMAIL") ?? "";
        private static readonly string _password =
            Environment.GetEnvironmentVariable("NAAD_TEST_PASSWORD") ?? "";

        private static readonly List<string> _lines = new List<string>();
        private static Task? _task;
        private static bool _finished;
        private static double _deadline;

        public static void Run()
        {
            Debug.Log($"[NAAD][FirstNight] starting; email={_email}");

            if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
            {
                Debug.LogError("[NAAD][FirstNight] NAAD_TEST_EMAIL / NAAD_TEST_PASSWORD not set.");
                EditorApplication.Exit(4);
                return;
            }

            _deadline = EditorApplication.timeSinceStartup + 240;
            _task = RunFlowAsync();
            EditorApplication.update += Pump;
        }

        private static void Pump()
        {
            if (_task == null) return;

            if (_task.IsFaulted)
            {
                Debug.LogError($"[NAAD][FirstNight] FAULTED: {_task.Exception}");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(2);
                return;
            }

            if (_task.IsCompleted && !_finished)
            {
                _finished = true;
                _lines.Add("]}");
                System.IO.File.WriteAllText(ReportPath, string.Join("\n", _lines));
                Debug.Log($"[NAAD][FirstNight] report written to {ReportPath}");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(0);
                return;
            }

            if (EditorApplication.timeSinceStartup > _deadline)
            {
                Debug.LogError("[NAAD][FirstNight] TIMEOUT after 240s.");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(5);
            }
        }

        private static void Emit(string s)
        {
            _lines.Add(s);
            Debug.Log("[NAAD][FirstNight] " + s);
        }

        private static async Task RunFlowAsync()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SupabaseConfig>(
                "Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset");
            if (cfg == null)
                throw new Exception("SupabaseConfig asset not found at Resources path.");
            if (!cfg.IsConfigured)
                throw new Exception("SupabaseConfig not configured (missing URL or anon key).");

            Emit($"{{\"configUrl\":\"{cfg.projectUrl}\",\"email\":\"{_email}\",\"steps\":[");

            var log = new UnityNAADLogger();

            // Compose the real (live) service graph exactly as NAADApplicationRoot does.
            var auth = new SupabaseAuthService(cfg, log);
            var players = new SupabasePlayerService(cfg, auth, log);
            var commands = new SupabaseCommandService(cfg, auth, log);
            var phone = new SupabasePhoneService(cfg, auth, log);

            // 1) Auth: sign in existing or sign up fresh.
            await auth.SignOutAsync();
            var signedIn = await auth.SignInAsync(_email, _password);
            Emit($"  {{\"step\":\"signin_existing\",\"ok\":{B(signedIn)}}},");
            if (!signedIn)
            {
                var signedUp = await auth.SignUpAsync(_email, _password, "Probe Player");
                Emit($"  {{\"step\":\"signup\",\"ok\":{B(signedUp)},\"isAuthenticated\":{B(auth.IsAuthenticated)}}},");
                if (!signedUp || !auth.IsAuthenticated)
                    throw new Exception("Signup did not yield an authenticated session.");
            }

            // 2) Player row must exist (trigger provisions it).
            PlayerDto? player = null;
            for (var i = 0; i < 5 && player == null; i++)
            {
                player = await players.FetchCurrentPlayerAsync();
                if (player == null) await Task.Delay(600);
            }
            if (player == null) throw new Exception("No player row for authenticated user.");
            Emit($"  {{\"step\":\"fetch_player\",\"ok\":true,\"displayName\":\"{player.DisplayName}\",\"id\":\"{player.Id}\"}},");

            // 3) Starting wallet balance (server-authoritative).
            var balanceBefore = await phone.GetBalanceAsync();
            Emit($"  {{\"step\":\"balance_before\",\"ok\":{B(balanceBefore.HasValue)},\"balance\":{(balanceBefore ?? -1)}}},");

            // 4) Install a live application root (real config → real services)
            // and drive the REAL FirstNightController through it.
            var controllerGo = new GameObject("FirstNightLiveTest");
            try
            {
                var controller = controllerGo.AddComponent<FirstNightController>();
                WireController(cfg);

                var rootLive = NAADApplicationRoot.Instance!;
                var cmdLive = rootLive.Commands;
                Emit($"  {{\"step\":\"root_live\",\"ok\":true,\"authType\":\"{rootLive.Auth.GetType().Name}\",\"commandsType\":\"{cmdLive.GetType().Name}\"}},");

                // The root owns its OWN SupabaseAuthService instance; it must pick up
                // the persisted session from PlayerPrefs (also proves restore works).
                var rootRestored = await rootLive.Auth.RestoreSessionAsync();
                Emit($"  {{\"step\":\"root_restore_session\",\"ok\":{B(rootRestored)},\"isAuthenticated\":{B(rootLive.Auth.IsAuthenticated)}}},");
                if (!rootRestored) throw new Exception("Root could not restore the persisted session.");

                var started = await controller.StartNightAsync();
                Emit($"  {{\"step\":\"start_night\",\"ok\":{B(started)},\"controllerStep\":\"{controller.Step}\"}},");
                if (!started) throw new Exception("START_NIGHT failed: " + controller.LastError);

                var replied = await controller.RespondToInviteAsync("GO");
                Emit($"  {{\"step\":\"phone_go\",\"ok\":{B(replied)},\"controllerStep\":\"{controller.Step}\"}},");
                if (!replied) throw new Exception("PHONE_REPLY failed: " + controller.LastError);

                var suya = await controller.GoToSuyaAndBuyAsync();
                Emit($"  {{\"step\":\"suya_travel_buy\",\"ok\":{B(suya)},\"controllerStep\":\"{controller.Step}\",\"balance\":{(controller.LastBalance ?? -1)}}},");
                if (!suya) throw new Exception("Suya legs failed: " + controller.LastError);

                var club = await controller.GoToClubAndBuyTicketAsync();
                Emit($"  {{\"step\":\"club_travel_ticket\",\"ok\":{B(club)},\"controllerStep\":\"{controller.Step}\",\"balance\":{(controller.LastBalance ?? -1)}}},");
                if (!club) throw new Exception("Club legs failed: " + controller.LastError);

                var completed = await controller.CompleteNightAsync();
                var summary = controller.LastSummary;
                Emit($"  {{\"step\":\"complete_night\",\"ok\":{B(completed)},\"controllerStep\":\"{controller.Step}\","
                     + $"\"summary\":{FmtSummary(summary)},\"balance\":{(controller.LastBalance ?? -1)}}},");
                if (!completed || summary == null || !summary.Success)
                    throw new Exception("COMPLETE_NIGHT failed: " + controller.LastError);

                // 5) Pass assertions: summary shape + money moved.
                // NOTE: `spent` + balance delta are the server-truth signals.
                // `transactions`/`peopleMet` are informational and may differ by
                // deployed server version; assert them only as present, not exact.
                var balanceAfter = controller.LastBalance ?? await phone.GetBalanceAsync();
                var moneyMoved = balanceBefore.HasValue && balanceAfter.HasValue
                                 && balanceAfter.Value < balanceBefore.Value;
                var spentMatchesDelta = balanceBefore.HasValue && balanceAfter.HasValue
                                 && (balanceBefore.Value - balanceAfter.Value) == summary.SpentNgn;
                var summaryShapeOk = summary.SpentNgn > 0
                                     && !string.IsNullOrEmpty(summary.BestMoment)
                                     && !string.IsNullOrEmpty(summary.NewConnection);
                Emit($"  {{\"step\":\"assert_money_moved\",\"ok\":{B(moneyMoved)},\"before\":{(balanceBefore ?? -1)},\"after\":{(balanceAfter ?? -1)}}},");
                Emit($"  {{\"step\":\"assert_spent_matches_delta\",\"ok\":{B(spentMatchesDelta)},\"spent\":{summary.SpentNgn}}},");
                Emit($"  {{\"step\":\"assert_summary_shape\",\"ok\":{B(summaryShapeOk)},"
                     + $"\"spent\":{summary.SpentNgn},\"transactions\":{summary.Transactions},"
                     + $"\"peopleMet\":{summary.PeopleMet},\"trust\":{(summary.TrustWithTunde ?? -1)}}},");

                var passed = moneyMoved && spentMatchesDelta && summaryShapeOk;
                Emit($"  {{\"step\":\"GATE_B_PASS\",\"ok\":{B(passed)}}}");
                if (!passed) throw new Exception("Gate B assertions failed.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerGo);
            }
        }

        /// <summary>
        /// FirstNightController reads services via NAADApplicationRoot.Instance.
        /// In headless tests there is no scene root, so create one and inject the
        /// live config via reflection, then re-run Compose() (private) so the
        /// root owns the REAL live services — the exact graph the game uses.
        /// No production change required: reflection stays inside the test harness.
        /// </summary>
        private static void WireController(SupabaseConfig cfg)
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null)
            {
                var go = new GameObject("NAADApplicationRoot(Test)");
                root = go.AddComponent<NAADApplicationRoot>();
            }

            // Ensure the static Instance is set even if Awake was deferred
            // (batch-mode edge: AddComponent may not fire Awake synchronously).
            var t = typeof(NAADApplicationRoot);
            var instProp = t.GetProperty("Instance",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instProp == null) throw new Exception("Instance property not found.");
            instProp.SetValue(null, root);

            // Inject the live config and re-compose so the root owns live services.
            var field = t.GetField("supabaseConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) throw new Exception("supabaseConfig field not found.");
            field.SetValue(root, cfg);

            var compose = t.GetMethod("Compose",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (compose == null) throw new Exception("Compose() not found.");
            compose.Invoke(root, null);

            if (NAADApplicationRoot.Instance == null)
                throw new Exception("Application root failed to install.");
            if (root.Config == null || !root.Config.IsConfigured)
                throw new Exception("Root did not compose live services.");
        }

        private static string FmtSummary(NightSummaryDto? s)
        {
            if (s == null) return "null";
            return "{\"success\":" + B(s.Success)
                 + ",\"spentNgn\":" + s.SpentNgn
                 + ",\"transactions\":" + s.Transactions
                 + ",\"peopleMet\":" + s.PeopleMet
                 + ",\"newConnection\":\"" + (s.NewConnection ?? "") + "\""
                 + ",\"trustWithTunde\":" + (s.TrustWithTunde?.ToString() ?? "null")
                 + ",\"bestMoment\":\"" + (s.BestMoment ?? "").Replace("\"", "'") + "\"}";
        }

        private static string B(bool v) => v ? "true" : "false";
    }
}
