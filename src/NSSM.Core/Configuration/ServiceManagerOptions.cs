namespace NSSM.Core.Configuration
{
    /// <summary>
    /// Options for configuring the service manager component.
    /// </summary>
    public class ServiceManagerOptions
    {
        /// <summary>
        /// Gets or sets the default timeout for service operations in milliseconds.
        /// </summary>
        public int DefaultTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the default service start type.
        /// </summary>
        public string DefaultStartType { get; set; } = "Automatic";

        /// <summary>
        /// Gets or sets the default service account.
        /// </summary>
        public string DefaultServiceAccount { get; set; } = "LocalSystem";
    }
}
