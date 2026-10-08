using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Core.State;
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
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    state.SetError("No session and no email/password configured on NAADBootstrap");
                    return;
                }

                authenticated = await root.Auth.SignInAsync(email, password);
            }

            if (!authenticated)
            {
                state.SetError("Authentication failed");
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
            await root.Scenes.LoadSceneAsync(nextSceneName);

            log.Info("Bootstrap", "Bootstrap sequence complete");
        }
    }
}
