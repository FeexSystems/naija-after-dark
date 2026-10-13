using System.Collections.Generic;

namespace NAAD.UI
{
    #region Phone Home Data Contracts

    /// <summary>
    /// Data contract for phone app item.
    /// </summary>
    public class PhoneAppItem
    {
        public string AppId { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Data contract for phone home screen.
    /// </summary>
    public class PhoneHomeData
    {
        public List<PhoneAppItem> Apps { get; set; } = new();
        public int BatteryLevel { get; set; } = 100;
        public bool IsCharging { get; set; }
        public string NetworkStatus { get; set; } = "4G";
    }

    #endregion

    #region Ping Messaging Data Contracts

    /// <summary>
    /// Data contract for message.
    /// </summary>
    public class MessageItem
    {
        public string MessageId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsFromPlayer { get; set; }
    }

    /// <summary>
    /// Data contract for contact.
    /// </summary>
    public class ContactItem
    {
        public string ContactId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
        public string LastSeen { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data contract for Ping messaging screen.
    /// </summary>
    public class PingMessagingData
    {
        public List<ContactItem> Contacts { get; set; } = new();
        public List<MessageItem> Messages { get; set; } = new();
        public string SelectedContactId { get; set; } = string.Empty;
        public int TotalUnread { get; set; }
    }

    #endregion

    #region Night Summary Data Contracts

    /// <summary>
    /// Data contract for night summary (reuses IPhoneService.NightSummaryDto)
    /// </summary>
    public class NightSummaryData
    {
        public bool Success { get; set; }
        public int SpentNgn { get; set; }
        public int Transactions { get; set; }
        public int PeopleMet { get; set; }
        public string NewConnection { get; set; } = string.Empty;
        public int? TrustWithTunde { get; set; }
        public string BestMoment { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }

    #endregion

    #region Main HUD Data Contracts

    /// <summary>
    /// Data contract for main HUD.
    /// </summary>
    public class MainHudData
    {
        public string PlayerName { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public int Balance { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;
        public bool HasActiveInteraction { get; set; }
        public string InteractionPrompt { get; set; } = string.Empty;
        public int NotificationCount { get; set; }
    }

    #endregion

    #region Bootstrap Data Contracts

    /// <summary>
    /// Data contract for bootstrap screen.
    /// </summary>
    public class BootstrapData
    {
        public bool IsAuthenticated { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string LoadingMessage { get; set; } = "Loading...";
        public float LoadingProgress { get; set; } = 0f;
    }

    #endregion

    #region Configuration Data Contracts

    /// <summary>
    /// Status of a single configuration dependency. Reports what the application
    /// root actually composed — never a fabricated success.
    /// </summary>
    public class ConfigurationItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data contract for the configuration/error screen.
    /// Backed by the real SupabaseConfig and NAADApplicationRoot composition.
    /// </summary>
    public class ConfigurationData
    {
        public bool IsConfigured { get; set; }
        public string ProjectUrl { get; set; } = string.Empty;
        public bool UsingLiveServices { get; set; }
        public List<ConfigurationItem> Services { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    #endregion

    #region Bottom Navigation Data Contracts

    /// <summary>
    /// Data contract for bottom navigation item.
    /// </summary>
    public class NavigationItem
    {
        public string TabId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public int BadgeCount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Data contract for bottom navigation.
    /// </summary>
    public class BottomNavigationData
    {
        public List<NavigationItem> Items { get; set; } = new();
        public string ActiveTabId { get; set; } = string.Empty;
    }

    #endregion
}
