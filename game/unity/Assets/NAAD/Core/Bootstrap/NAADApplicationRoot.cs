using NAAD.Core.Logging;
using NAAD.Core.State;
using NAAD.Networking;
using NAAD.Networking.Config;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Core.Bootstrap
{
    /// <summary>
    /// Persistent application root. Owns service references via composition.
    /// Assign SupabaseConfig in the Inspector to use live Auth (Gate 4).
    /// Without config, falls back to stubs (Gate 3 offline).
    /// </summary>
    public sealed class NAADApplicationRoot : MonoBehaviour
    {
        public static NAADApplicationRoot? Instance { get; private set; }

        [SerializeField] private SupabaseConfig? supabaseConfig;

        public INAADLogger Logger { get; private set; } = null!;
        public GameStateManager GameState { get; private set; } = null!;
        public IAuthService Auth { get; private set; } = null!;
        public IPlayerService Players { get; private set; } = null!;
        public IWorldStateService World { get; private set; } = null!;
        public ISceneLoader Scenes { get; private set; } = null!;
        public SupabaseConfig? Config => supabaseConfig;

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
            Scenes = new SceneLoader(Logger);

            if (supabaseConfig != null && supabaseConfig.IsConfigured)
            {
                Auth = new SupabaseAuthService(supabaseConfig, Logger);
                Players = new SupabasePlayerService(supabaseConfig, Auth, Logger);
                World = new SupabaseWorldStateService(supabaseConfig, Auth, Logger);
                Logger.Info("Root", "Application root composed (Supabase live services)");
            }
            else
            {
                Auth = new StubAuthService(Logger);
                Players = new StubPlayerService(Auth, Logger);
                World = new StubWorldStateService(Logger);
                Logger.Warn("Root", "SupabaseConfig missing — using stub services");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
