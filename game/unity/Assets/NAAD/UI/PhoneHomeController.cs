using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for phone home screen.
    /// Displays app launcher with notification badges.
    /// </summary>
    public class PhoneHomeController : BaseScreenController
    {
        private IPhoneService? _phoneService;
        private PhoneHomeData _data = new();

        protected override void Initialize()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;

            _phoneService = root.Phone;

            // Initialize app list
            InitializeApps();
        }

        private void InitializeApps()
        {
            _data.Apps = new System.Collections.Generic.List<PhoneAppItem>
            {
                new PhoneAppItem
                {
                    AppId = "ping",
                    AppName = "Ping",
                    IconName = "icon_ping",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "naijagram",
                    AppName = "NaijaGram",
                    IconName = "icon_naijagram",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "nightpulse",
                    AppName = "NightPulse",
                    IconName = "icon_nightpulse",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "quickcash",
                    AppName = "QuickCash",
                    IconName = "icon_quickcash",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "ridenow",
                    AppName = "RideNow",
                    IconName = "icon_ridenow",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "markethub",
                    AppName = "MarketHub",
                    IconName = "icon_markethub",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "metromap",
                    AppName = "MetroMap",
                    IconName = "icon_metromap",
                    UnreadCount = 0,
                    IsEnabled = true
                },
                new PhoneAppItem
                {
                    AppId = "music",
                    AppName = "Music",
                    IconName = "icon_music",
                    UnreadCount = 0,
                    IsEnabled = true
                }
            };
        }

        protected override async void LoadData()
        {
            SetState(ScreenState.Loading);

            try
            {
                // Load balance
                if (_phoneService != null)
                {
                    var balance = await _phoneService.GetBalanceAsync();
                    if (balance.HasValue)
                    {
                        // Balance is displayed in QuickCash app
                    }

                    // Load messages for Ping unread count
                    var messagesJson = await _phoneService.ListMessagesJsonAsync();
                    if (!string.IsNullOrEmpty(messagesJson))
                    {
                        // Parse messages and update unread count
                        // TODO: Implement JSON parsing
                    }
                }

                // Set battery and network status (simulated for now)
                _data.BatteryLevel = 85;
                _data.IsCharging = false;
                _data.NetworkStatus = "4G";

                SetState(ScreenState.Success);
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to load phone data: {ex.Message}");
            }
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            switch (newState)
            {
                case ScreenState.Loading:
                    ShowLoadingView();
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
            // Show loading spinner
        }

        private void ShowSuccessView()
        {
            // Show app grid
            // Update notification badges
            // Update battery indicator
            // Update network status
        }

        private void ShowErrorView()
        {
            // Show error message
            // Display retry button
        }

        /// <summary>
        /// Handle app tap.
        /// </summary>
        public void OnAppTapped(string appId)
        {
            var app = _data.Apps.Find(a => a.AppId == appId);
            if (app != null && app.IsEnabled)
            {
                // Navigate to app screen
                // TODO: Implement navigation
            }
        }

        /// <summary>
        /// Update unread count for an app.
        /// </summary>
        public void UpdateUnreadCount(string appId, int count)
        {
            var app = _data.Apps.Find(a => a.AppId == appId);
            if (app != null)
            {
                app.UnreadCount = count;
                UpdateHud();
            }
        }

        private void UpdateHud()
        {
            // Update notification badges
        }

        /// <summary>
        /// Get current phone home data for UI binding.
        /// </summary>
        public PhoneHomeData GetData()
        {
            return _data;
        }

        /// <summary>
        /// Refresh phone data.
        /// </summary>
        public void Refresh()
        {
            SetState(ScreenState.Loading);
            LoadData();
        }
    }
}
