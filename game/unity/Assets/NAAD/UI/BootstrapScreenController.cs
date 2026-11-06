using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;
using NAAD.UI.Scripts;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for bootstrap/loading/error screen.
    /// Handles initial loading, authentication check, and error display.
    /// </summary>
    public class BootstrapScreenController : BaseScreenController
    {
        private IAuthService? _authService;
        private IPlayerService? _playerService;
        private BootstrapData _data = new();

        protected override void Initialize()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;

            _authService = root.Auth;
            _playerService = root.Players;
        }

        protected override async void LoadData()
        {
            SetState(ScreenState.Loading, "Initializing...");

            try
            {
                // Check authentication
                if (_authService == null)
                {
                    SetState(ScreenState.Error, "Authentication service not available");
                    return;
                }

                _data.LoadingMessage = "Checking authentication...";
                _data.LoadingProgress = 0.2f;

                var isAuthenticated = await _authService.RestoreSessionAsync();

                if (isAuthenticated)
                {
                    _data.IsAuthenticated = true;
                    _data.LoadingMessage = "Loading player profile...";
                    _data.LoadingProgress = 0.5f;

                    // Load player data
                    if (_playerService != null)
                    {
                        var player = await _playerService.FetchCurrentPlayerAsync();
                        if (player != null)
                        {
                            _data.PlayerName = player.DisplayName;
                        }
                    }

                    _data.LoadingMessage = "Loading world state...";
                    _data.LoadingProgress = 0.8f;

                    // Load world state (if implemented)
                    // TODO: Add world state loading

                    _data.LoadingProgress = 1.0f;
                    SetState(ScreenState.Success);
                }
                else
                {
                    _data.IsAuthenticated = false;
                    _data.LoadingMessage = "Please sign in to continue";
                    SetState(ScreenState.Empty);
                }
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to initialize: {ex.Message}");
            }
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            // Update UI based on state
            switch (newState)
            {
                case ScreenState.Loading:
                    ShowLoadingView();
                    break;
                case ScreenState.Empty:
                    ShowEmptyView();
                    break;
                case ScreenState.Success:
                    ShowSuccessView();
                    break;
                case ScreenState.Error:
                    ShowErrorView();
                    break;
            }
        }

        private void ShowLoadingView()
        {
            if (View is not BootstrapView view) return;
            view.ShowLoading();
            view.SetStatus("Starting up…");
        }

        private void ShowEmptyView()
        {
            // Nothing loaded yet: show the content shell and ask the player to sign in.
            if (View is not BootstrapView view) return;
            view.ShowContent();
            view.SetStatus("Sign in to continue.");
        }

        private void ShowSuccessView()
        {
            if (View is not BootstrapView view) return;
            view.ShowContent();
            view.SetStatus("Ready.");
        }

        private void ShowErrorView()
        {
            if (View is not BootstrapView view) return;
            view.ShowError(string.IsNullOrEmpty(ErrorMessage) ? "Startup failed." : ErrorMessage);
        }

        /// <summary>Wire the view's Retry button to <see cref="Retry"/>.</summary>
        protected override void OnViewBound(NAAD.UI.Scripts.ScreenViewBase view)
        {
            base.OnViewBound(view);

            if (view is BootstrapView bootstrap)
                bootstrap.OnRetry += Retry;
        }

        /// <summary>
        /// Get current bootstrap data for UI binding.
        /// </summary>
        public BootstrapData GetData()
        {
            return _data;
        }

        /// <summary>
        /// Retry initialization after error.
        /// </summary>
        public void Retry()
        {
            SetState(ScreenState.Retry);
            LoadData();
        }
    }
}
