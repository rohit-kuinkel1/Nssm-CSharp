namespace NSSM.Core.Configuration
{

    /// <summary>
    /// Options for configuring UI-related settings.
    /// </summary>
    public class UIOptions
    {
        /// <summary>
        /// Gets or sets whether to confirm service deletion.
        /// </summary>
        public bool ConfirmServiceDeletion { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show advanced options by default.
        /// </summary>
        public bool ShowAdvancedOptionsByDefault { get; set; } = false;

        /// <summary>
        /// Gets or sets the refresh interval for service status in milliseconds.
        /// </summary>
        public int StatusRefreshIntervalMs { get; set; } = 5000;
    }
}
