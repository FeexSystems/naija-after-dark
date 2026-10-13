namespace NAAD.UI
{
    /// <summary>
    /// Screen state enumeration for UI state management.
    /// Each screen transitions through these states during its lifecycle.
    /// </summary>
    public enum ScreenState
    {
        /// <summary>
        /// Screen is loading data from service.
        /// Show loading indicator.
        /// </summary>
        Loading,

        /// <summary>
        /// Screen has no data to display.
        /// Show empty state message.
        /// </summary>
        Empty,

        /// <summary>
        /// Screen has data and is ready for interaction.
        /// Show content.
        /// </summary>
        Success,

        /// <summary>
        /// Screen encountered an error.
        /// Show error message with retry option.
        /// </summary>
        Error,

        /// <summary>
        /// Screen is retrying after an error.
        /// Show loading indicator.
        /// </summary>
        Retry
    }
}
