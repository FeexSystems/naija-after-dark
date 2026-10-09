using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;
using NAAD.Networking.Supabase;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Live end-to-end harness for the real SupabaseAuthService + SupabasePlayerService.
    /// Run headless:
    ///   Unity -batchmode -quit -projectPath game/unity \
    ///     -executeMethod NAAD.Editor.AuthLiveTest.Run
    /// Requires Resources/SupabaseConfig.asset with a valid anon key.
    /// Writes a JSON report to Library/auth-live-report.json and exits 0/1.
    /// </summary>
    public static class AuthLiveTest
    {
        private const string ReportPath = "Library/auth-live-report.json";
        private static readonly string _email =
            Environment.GetEnvironmentVariable("NAAD_TEST_EMAIL") ?? "";
        private static readonly string _password =
            Environment.GetEnvironmentVariable("NAAD_TEST_PASSWORD") ?? "";
        private static readonly string _displayName = "Probe Player";

        private static readonly System.Collections.Generic.List<string> _lines =
            new System.Collections.Generic.List<string>();
        private static Task? _task;
        private static bool _finished;
        private static double _deadline;

        // SupabaseHttp awaits Task.Yield(), so continuations need the Unity player loop.
        // Blocking with .GetAwaiter().GetResult() would deadlock — drive via update instead.
        public static void Run()
        {
            Debug.Log($"[NAAD][AuthLiveTest] starting; email={_email}");

            if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
            {
                Debug.LogError("[NAAD][AuthLiveTest] NAAD_TEST_EMAIL / NAAD_TEST_PASSWORD not set.");
                EditorApplication.Exit(4);
                return;
            }

            _deadline = EditorApplication.timeSinceStartup + 150;
            _task = RunAsync();
            EditorApplication.update += Pump;
        }

        private static void Pump()
        {
            if (_task == null) return;

            if (_task.IsFaulted)
            {
                Debug.LogError($"[NAAD][AuthLiveTest] FAULTED: {_task.Exception}");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(2);
                return;
            }

            if (_task.IsCompleted && !_finished)
            {
                _finished = true;
                _lines.Add("]}");
                System.IO.File.WriteAllText(ReportPath, string.Join("\n", _lines));
                Debug.Log($"[NAAD][AuthLiveTest] report written to {ReportPath}");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(0);
                return;
            }

            if (EditorApplication.timeSinceStartup > _deadline)
            {
                Debug.LogError("[NAAD][AuthLiveTest] TIMEOUT after 150s.");
                EditorApplication.update -= Pump;
                EditorApplication.Exit(5);
            }
        }

        private static void Emit(string s)
        {
            _lines.Add(s);
            Debug.Log("[NAAD][AuthLiveTest] " + s);
        }

        private static async Task RunAsync()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SupabaseConfig>(
                "Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset");
            if (cfg == null)
                throw new Exception("SupabaseConfig asset not found at Resources path.");

            Emit($"{{\"configUrl\":\"{cfg.projectUrl}\",\"configured\":{B(cfg.IsConfigured)},\"steps\":[");

            var log = new UnityNAADLogger();
            var auth = new SupabaseAuthService(cfg, log);
            var players = new SupabasePlayerService(cfg, auth, log);

            // 0) Pre-clean: make sure no stale session exists.
            await auth.SignOutAsync();

            // 1) SignIn first — if the account already exists from a prior run, reuse it.
            var signInFirst = await auth.SignInAsync(_email, _password);
            Emit($"  {{\"step\":\"signin_existing\",\"ok\":{B(signInFirst)}}},");

            var signUpOk = false;
            if (!signInFirst)
            {
                // 2) SignUpAsync — real account creation.
                signUpOk = await auth.SignUpAsync(_email, _password, _displayName);
                Emit($"  {{\"step\":\"signup\",\"ok\":{B(signUpOk)},\"isAuthenticated\":{B(auth.IsAuthenticated)}}},");
            }

            // 3) RestoreSessionAsync (reads PlayerPrefs, validates /user).
            var restored = await auth.RestoreSessionAsync();
            var token = await auth.GetAccessTokenAsync();
            var pid = await auth.GetPlayerIdAsync();
            Emit($"  {{\"step\":\"restore_session\",\"ok\":{B(restored)},\"hasToken\":{B(!string.IsNullOrEmpty(token))},\"playerId\":\"{pid}\"}},");

            // 4) FetchCurrentPlayerAsync — the trigger should have provisioned the row.
            NAAD.Networking.PlayerDto? player = null;
            for (var i = 0; i < 5 && player == null; i++)
            {
                player = await players.FetchCurrentPlayerAsync();
                if (player == null) await Task.Delay(500);
            }
            Emit($"  {{\"step\":\"fetch_player\",\"ok\":{B(player != null)},\"displayName\":\"{player?.DisplayName}\",\"id\":\"{player?.Id}\"}},");

            // 5) SignOutAsync — must clear local session.
            var outOk = await auth.SignOutAsync();
            var tokenAfter = await auth.GetAccessTokenAsync();
            Emit($"  {{\"step\":\"signout\",\"ok\":{B(outOk)},\"tokenCleared\":{B(string.IsNullOrEmpty(tokenAfter))},\"isAuthenticated\":{B(auth.IsAuthenticated)}}},");

            // 6) Restore after signout must fail (nothing stored).
            var restoreAfterOut = await auth.RestoreSessionAsync();
            Emit($"  {{\"step\":\"restore_after_signout_rejected\",\"ok\":{B(!restoreAfterOut)}}},");

            // 7) Sign back in — proves the round trip.
            var reSignIn = await auth.SignInAsync(_email, _password);
            Emit($"  {{\"step\":\"signin_again\",\"ok\":{B(reSignIn)},\"isAuthenticated\":{B(auth.IsAuthenticated)}}}");
        }

        private static string B(bool v) => v ? "true" : "false";
    }
}
