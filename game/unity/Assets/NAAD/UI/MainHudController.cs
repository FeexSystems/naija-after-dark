using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for main in-game HUD.
    /// Displays player info, location, balance, and interaction prompts.
    /// </summary>
    public class MainHudController : BaseScreenController
    {
        private IPlayerService? _playerService;
        private IPhoneService? _phoneService;
        private MainHudData _data = new();

        protected override void Initialize()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;

            _playerService = root.Players;
            _phoneService = root.Phone;
        }

        protected override async void LoadData()
        {
            SetState(ScreenState.Loading);

            try
            {
                // Load player data
                if (_playerService != null)
                {
                    var player = await _playerService.FetchCurrentPlayerAsync();
                    if (player != null)
                    {
                        _data.PlayerName = player.DisplayName;
                    }
                }

                // Load balance
                if (_phoneService != null)
                {
                    var balance = await _phoneService.GetBalanceAsync();
                    if (balance.HasValue)
                    {
                        _data.Balance = balance.Value;
                    }
                }

                // Set default values for location and time
                _data.CurrentLocation = "Apartment";
                _data.TimeOfDay = "Evening";
                _data.HasActiveInteraction = false;

                SetState(ScreenState.Success);
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to load HUD data: {ex.Message}");
            }
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            switch (newState)
            {
                case ScreenState.Loading:
                    HideHud();
                    break;
                case ScreenState.Success:
                    ShowHud();
                    UpdateHud();
                    break;
                case ScreenState.Error:
                    HideHud();
                    break;
            }
        }

        private void ShowHud()
        {
            // Show HUD canvas
        }

        private void HideHud()
        {
            // Hide HUD canvas
        }

        private void UpdateHud()
        {
            // Update player name display
            // Update location display
            // Update balance display
            // Update time of day display
            // Update notification badge
        }

        /// <summary>
        /// Update current location.
        /// </summary>
        public void UpdateLocation(string location)
        {
            _data.CurrentLocation = location;
            UpdateHud();
        }

        /// <summary>
        /// Update time of day.
        /// </summary>
        public void UpdateTimeOfDay(string timeOfDay)
        {
            _data.TimeOfDay = timeOfDay;
            UpdateHud();
        }

        /// <summary>
        /// Update balance.
        /// </summary>
        public async void UpdateBalance()
        {
            if (_phoneService != null)
            {
                var balance = await _phoneService.GetBalanceAsync();
                if (balance.HasValue)
                {
                    _data.Balance = balance.Value;
                    UpdateHud();
                }
            }
        }

        /// <summary>
        /// Show interaction prompt.
        /// </summary>
        public void ShowInteractionPrompt(string prompt)
        {
            _data.HasActiveInteraction = true;
            _data.InteractionPrompt = prompt;
            UpdateHud();
        }

        /// <summary>
        /// Hide interaction prompt.
        /// </summary>
        public void HideInteractionPrompt()
        {
            _data.HasActiveInteraction = false;
            _data.InteractionPrompt = string.Empty;
            UpdateHud();
        }

        /// <summary>
        /// Update notification count.
        /// </summary>
        public void UpdateNotificationCount(int count)
        {
            _data.NotificationCount = count;
            UpdateHud();
        }

        /// <summary>
        /// Get current HUD data for UI binding.
        /// </summary>
        public MainHudData GetData()
        {
            return _data;
        }
    }
}
