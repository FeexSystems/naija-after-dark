using NAAD.Core.Bootstrap;
using NAAD.Networking.Config;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for the configuration/error screen.
    ///
    /// Reports what the application root actually composed — live Supabase services
    /// or stub fallbacks — and exposes a retry that re-reads the real configuration.
    /// Never fabricates a success: if the config asset is missing, unconfigured, or
    /// the root is absent, the screen shows Error, not Success.
    /// Extends BaseScreenController like every other NAAD screen, but does not call
    /// any backend service: configuration truth is local (the SupabaseConfig asset
    /// and the composition the root performed in Awake).
    /// </summary>
    public class ConfigurationUIController : BaseScreenController
    {
        private ConfigurationData _data = new();

        protected override void Initialize()
        {
            SetState(ScreenState.Loading, "Checking configuration...");
        }

        protected override void LoadData()
        {
            Refresh();
        }

        /// <summary>
        /// Re-read configuration and composition state. Called on load and on retry,
        /// so a config fixed at runtime (asset assigned, keys filled in) is picked up
        /// without a scene reload.
        /// </summary>
        public void Refresh()
        {
            _data = new ConfigurationData();

            var root = NAADApplicationRoot.Instance;

            // The root composes in Awake. In batch-mode or early frames it can be
            // absent; that is an error state, not a silent pass.
            if (root == null)
            {
                _data.ErrorMessage = "Application root not initialised. Cannot determine configuration.";
                SetState(ScreenState.Error, _data.ErrorMessage);
                Render();
                return;
            }

            var config = root.Config;

            if (config == null)
            {
                _data.ErrorMessage = "SupabaseConfig asset is not assigned to NAADApplicationRoot.";
                SetState(ScreenState.Error, _data.ErrorMessage);
                Render();
                return;
            }

            _data.IsConfigured = config.IsConfigured;
            _data.ProjectUrl = config.projectUrl;
            _data.UsingLiveServices = config.IsConfigured;

            // Report each service slot the root owns, and whether it is a live
            // Supabase implementation or a stub fallback. The root's compose()
            // leaves Dialogue null when unconfigured — that is reported, not hidden.
            _data.Services.Add(new ConfigurationItem
            {
                Name = "Auth",
                IsAvailable = IsLive(root.Auth),
                Detail = DescribeService(root.Auth, "SupabaseAuthService", "StubAuthService")
            });
            _data.Services.Add(new ConfigurationItem
            {
                Name = "Players",
                IsAvailable = IsLive(root.Players),
                Detail = DescribeService(root.Players, "SupabasePlayerService", "StubPlayerService")
            });
            _data.Services.Add(new ConfigurationItem
            {
                Name = "World",
                IsAvailable = IsLive(root.World),
                Detail = DescribeService(root.World, "SupabaseWorldStateService", "StubWorldStateService")
            });
            _data.Services.Add(new ConfigurationItem
            {
                Name = "Commands",
                IsAvailable = IsLive(root.Commands),
                Detail = DescribeService(root.Commands, "SupabaseCommandService", "StubCommandService")
            });
            _data.Services.Add(new ConfigurationItem
            {
                Name = "Phone",
                IsAvailable = IsLive(root.Phone),
                Detail = DescribeService(root.Phone, "SupabasePhoneService", "StubPhoneService")
            });
            _data.Services.Add(new ConfigurationItem
            {
                Name = "Dialogue",
                IsAvailable = root.Dialogue != null,
                Detail = root.Dialogue != null
                    ? "SupabaseDialogueService (live)"
                    : "Not available (requires configured SupabaseConfig)"
            });

            if (config.IsConfigured)
            {
                SetState(ScreenState.Success);
            }
            else
            {
                _data.ErrorMessage = "SupabaseConfig is missing a project URL or anon key.";
                SetState(ScreenState.Error, _data.ErrorMessage);
            }

            Render();
        }

        /// <summary>
        /// Retry re-reads configuration. This is deliberately different from
        /// BootstrapScreenController.Retry: there is no network call to re-issue,
        /// only configuration to re-read.
        /// </summary>
        public void Retry()
        {
            SetState(ScreenState.Retry, "Re-checking configuration...");
            Refresh();
        }

        /// <summary>
        /// Get current configuration data for UI binding.
        /// </summary>
        public ConfigurationData GetData()
        {
            return _data;
        }

        /// <summary>
        /// A service is "live" when its concrete type is a Supabase implementation
        /// rather than a stub. Checks the type name so the check survives refactors
        /// of namespaces.
        /// </summary>
        private static bool IsLive(object? service)
        {
            return service != null && service.GetType().Name.StartsWith("Supabase");
        }

        private static string DescribeService(object? service, string liveName, string stubName)
        {
            if (service == null)
                return "Not composed";
            return service.GetType().Name == liveName
                ? $"{liveName} (live)"
                : $"{stubName} (stub fallback — configure SupabaseConfig for live services)";
        }

        /// <summary>
        /// Render is where the view layer binds. The views themselves are still
        /// pending (see UI_IMPLEMENTATION_REPORT.md); this override exists so the
        /// base-class hook is wired and the data contract is the single binding
        /// surface when the view lands.
        /// </summary>
        protected override void Render()
        {
            // View binding pending. All state is available via GetData().
        }
    }
}
