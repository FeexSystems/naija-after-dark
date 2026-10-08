using NAAD.Core.Logging;
using NAAD.Core.State;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.Core.Bootstrap
{
    /// <summary>
    /// Persistent application root. Owns service references via composition,
    /// not static service-locator spaghetti.
    /// </summary>
    public sealed class NAADApplicationRoot : MonoBehaviour
    {
        public static NAADApplicationRoot? Instance { get; private set; }

        public INAADLogger Logger { get; private set; } = null!;
        public GameStateManager GameState { get; private set; } = null!;
        public IAuthService Auth { get; private set; } = null!;
        public IPlayerService Players { get; private set; } = null!;
        public IWorldStateService World { get; private set; } = null!;
        public ISceneLoader Scenes { get; private set; } = null!;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Compose();
        }

        private void Compose()
        {
            Logger = new UnityNAADLogger();
            GameState = new GameStateManager(Logger);
            Auth = new StubAuthService(Logger);
            Players = new StubPlayerService(Auth, Logger);
            World = new StubWorldStateService(Logger);
            Scenes = new SceneLoader(Logger);
            Logger.Info("Root", "Application root composed (stub services)");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
