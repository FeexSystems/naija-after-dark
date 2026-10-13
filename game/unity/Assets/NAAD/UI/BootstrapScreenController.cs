using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;

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
            // Show loading spinner with progress
            // Display loading message
            // Update progress bar
        }

        private void ShowEmptyView()
        {
            // Show sign-in prompt
            // Display sign-in button
        }

        private void ShowSuccessView()
        {
            // Hide bootstrap screen
            // Show main game UI
        }

        private void ShowErrorView()
        {
            // Show error message
            // Display retry button
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
