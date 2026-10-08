using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Core.State;
using UnityEngine;

namespace NAAD.Core.Bootstrap
{
    /// <summary>
    /// Gate 3.1 entry point. Runs:
    /// BOOT → AUTH → PLAYER → WORLD → SCENE
    /// Attach to a GameObject in the Bootstrap scene.
    /// Requires NAADApplicationRoot in the same scene (or already present).
    /// </summary>
    public sealed class NAADBootstrap : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "Main";
        [SerializeField] private string stubEmail = "gate3@naad.local";
        [SerializeField] private string stubPassword = "not-a-secret";

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

            // BOOT
            state.SetPhase(GamePhase.Boot);
            log.Info("Bootstrap", "BOOT");

            // AUTH
            state.SetPhase(GamePhase.Auth);
            log.Info("Bootstrap", "AUTH");
            var signedIn = await root.Auth.SignInAsync(stubEmail, stubPassword);
            if (!signedIn)
            {
                state.SetError("Authentication failed");
                return;
            }

            // PLAYER
            state.SetPhase(GamePhase.Player);
            log.Info("Bootstrap", "PLAYER");
            var player = await root.Players.FetchCurrentPlayerAsync();
            if (player == null)
            {
                state.SetError("Failed to load player");
                return;
            }
            state.SetPlayer(player);

            // WORLD
            state.SetPhase(GamePhase.World);
            log.Info("Bootstrap", "WORLD");
            var world = await root.World.FetchWorldStateAsync();
            if (world == null)
            {
                state.SetError("Failed to load world state");
                return;
            }
            state.SetWorldState(world);

            // SCENE
            state.SetPhase(GamePhase.SceneReady);
            log.Info("Bootstrap", "SCENE");
            await root.Scenes.LoadSceneAsync(nextSceneName);

            log.Info("Bootstrap", "Bootstrap sequence complete");
        }
    }
}
