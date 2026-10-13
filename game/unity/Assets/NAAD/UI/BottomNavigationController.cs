using System;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for bottom navigation bar.
    /// Manages tab switching and badge notifications.
    /// </summary>
    public class BottomNavigationController : BaseScreenController
    {
        private BottomNavigationData _data = new();

        public event Action<string> OnTabChanged;

        protected override void Initialize()
        {
            // Initialize navigation items
            InitializeNavigationItems();
        }

        private void InitializeNavigationItems()
        {
            _data.Items = new System.Collections.Generic.List<NavigationItem>
            {
                new NavigationItem
                {
                    TabId = "home",
                    Label = "Home",
                    IconName = "icon_home",
                    BadgeCount = 0,
                    IsActive = true
                },
                new NavigationItem
                {
                    TabId = "social",
                    Label = "Social",
                    IconName = "icon_social",
                    BadgeCount = 0,
                    IsActive = false
                },
                new NavigationItem
                {
                    TabId = "city",
                    Label = "City",
                    IconName = "icon_city",
                    BadgeCount = 0,
                    IsActive = false
                },
                new NavigationItem
                {
                    TabId = "life",
                    Label = "Life",
                    IconName = "icon_life",
                    BadgeCount = 0,
                    IsActive = false
                },
                new NavigationItem
                {
                    TabId = "profile",
                    Label = "Profile",
                    IconName = "icon_profile",
                    BadgeCount = 0,
                    IsActive = false
                }
            };

            _data.ActiveTabId = "home";
        }

        protected override void LoadData()
        {
            // Navigation data is static, no async loading needed
            SetState(ScreenState.Success);
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            switch (newState)
            {
                case ScreenState.Success:
                    ShowNavigation();
                    break;
            }
        }

        private void ShowNavigation()
        {
            // Show navigation bar
            // Update active tab highlight
            // Update badge counts
        }

        /// <summary>
        /// Switch to a different tab.
        /// </summary>
        public void SwitchTab(string tabId)
        {
            var item = _data.Items.Find(i => i.TabId == tabId);
            if (item == null) return;

            // Update active state
            foreach (var navItem in _data.Items)
            {
                navItem.IsActive = navItem.TabId == tabId;
            }

            _data.ActiveTabId = tabId;
            OnTabChanged?.Invoke(tabId);
            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            // Update active tab highlight
            // Update badge counts
        }

        /// <summary>
        /// Update badge count for a tab.
        /// </summary>
        public void UpdateBadgeCount(string tabId, int count)
        {
            var item = _data.Items.Find(i => i.TabId == tabId);
            if (item != null)
            {
                item.BadgeCount = count;
                UpdateNavigation();
            }
        }

        /// <summary>
        /// Get current navigation data for UI binding.
        /// </summary>
        public BottomNavigationData GetData()
        {
            return _data;
        }

        /// <summary>
        /// Get currently active tab ID.
        /// </summary>
        public string GetActiveTab()
        {
            return _data.ActiveTabId;
        }
    }
}
