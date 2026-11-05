using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Core.State;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.Core.Bootstrap
{
    /// <summary>
    /// Entry point. Runs:
    /// BOOT → AUTH (restore or sign-in) → PLAYER → WORLD → SCENE
    /// </summary>
    public sealed class NAADBootstrap : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "Main";
        [Header("Dev sign-in (used when no restored session)")]
        [SerializeField] private string email = "";
        [SerializeField] private string password = "";
        [SerializeField] private bool preferRestoreSession = true;

        /// <summary>Environment variables consulted when the serialized credentials are blank.</summary>
        public const string DevEmailEnvVar = "NAAD_DEV_EMAIL";
        public const string DevPasswordEnvVar = "NAAD_DEV_PASSWORD";

        /// <summary>
        /// Serialized value first, then the environment.
        /// <para>
        /// The Bootstrap scene shipped with both fields empty and there is no auth screen,
        /// so a player had no way to sign in at all. Environment variables let a developer
        /// run the night against a real project without committing a password to the scene.
        /// </para>
        /// </summary>
        private static string ResolveCredential(string serialized, string envVar)
        {
            if (!string.IsNullOrWhiteSpace(serialized)) return serialized;
            // Fully qualified: the project has a NAAD.Environment namespace that shadows System.Environment.
            var fromEnv = System.Environment.GetEnvironmentVariable(envVar);
            return string.IsNullOrWhiteSpace(fromEnv) ? serialized : fromEnv;
        }

        private async void Start()
        {
            try
            {
                await RunAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NAAD][Bootstrap] Fatal: {ex}");
            }
        }

        private async Task RunAsync()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null)
            {
                var go = new GameObject("NAADApplicationRoot");
                root = go.AddComponent<NAADApplicationRoot>();
            }

            var log = root.Logger;
            var state = root.GameState;

            state.SetPhase(GamePhase.Boot);
            log.Info("Bootstrap", "BOOT");

            state.SetPhase(GamePhase.Auth);
            log.Info("Bootstrap", "AUTH");

            var authenticated = false;
            if (preferRestoreSession)
                authenticated = await root.Auth.RestoreSessionAsync();

            if (!authenticated)
            {
                var devEmail = ResolveCredential(email, DevEmailEnvVar);
                var devPassword = ResolveCredential(password, DevPasswordEnvVar);

                if (string.IsNullOrWhiteSpace(devEmail) || string.IsNullOrWhiteSpace(devPassword))
                {
                    var message =
                        "No session and no dev credentials. Set Email/Password on the " +
                        "NAADBootstrap component in Assets/Scenes/Bootstrap.unity, or export " +
                        $"{DevEmailEnvVar} and {DevPasswordEnvVar}. See docs/SUPABASE_SETUP_RUNBOOK.md.";
                    state.SetError(message);
                    log.Error("Bootstrap", message);
                    return;
                }

                authenticated = await root.Auth.SignInAsync(devEmail, devPassword);
            }

            if (!authenticated)
            {
                var message =
                    $"Sign-in failed for '{ResolveCredential(email, DevEmailEnvVar)}'. Check the account exists " +
                    "in the Supabase project, that email confirmation is off (or the address is confirmed), " +
                    "and that projectUrl/anonKey in SupabaseConfig.asset point at that project.";
                state.SetError(message);
                log.Error("Bootstrap", message);
                return;
            }

            state.SetPhase(GamePhase.Player);
            log.Info("Bootstrap", "PLAYER");

            // Brief retry — trigger may create player row slightly after signup
            PlayerDto? player = null;
            for (var attempt = 0; attempt < 5; attempt++)
            {
                player = await root.Players.FetchCurrentPlayerAsync();
                if (player != null) break;
                await Task.Delay(400);
            }

            if (player == null)
            {
                state.SetError("Failed to load player profile");
                return;
            }
            state.SetPlayer(player);

            state.SetPhase(GamePhase.World);
            log.Info("Bootstrap", "WORLD");
            var world = await root.World.FetchWorldStateAsync();
            if (world == null)
            {
                state.SetError("Failed to load world state");
                return;
            }
            state.SetWorldState(world);

            state.SetPhase(GamePhase.SceneReady);
            log.Info("Bootstrap", "SCENE");
            if (!string.IsNullOrWhiteSpace(nextSceneName))
                await root.Scenes.LoadSceneAsync(nextSceneName);

            log.Info("Bootstrap", "Bootstrap sequence complete");
        }
    }
}
