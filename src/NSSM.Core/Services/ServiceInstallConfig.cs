namespace NSSM.Core.Services
{
    /// <summary>
    /// Configuration for installing a new Windows service.
    /// </summary>
    public class ServiceInstallConfig
    {
        /// <summary>
        /// Gets or sets the name of the service
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the service
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the service
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to the executable that will run as a service
        /// </summary>
        public string BinaryPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the arguments to pass to the executable
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account under which the service should run
        /// </summary>
        public string Account { get; set; } = "LocalSystem";

        /// <summary>
        /// Gets or sets the startup type for the service
        /// </summary>
        public string StartType { get; set; } = "Automatic";

        /// <summary>
        /// Gets or sets whether the service should start immediately after installation
        /// </summary>
        public bool StartImmediately { get; set; } = false;
    }
}
