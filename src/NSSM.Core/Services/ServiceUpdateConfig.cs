namespace NSSM.Core.Services
{
    /// <summary>
    /// Configuration for updating an existing Windows service.
    /// </summary>
    public class ServiceUpdateConfig : ServiceInstallConfig
    {
        /// <summary>
        /// Gets or sets the existing name of the service that should be updated
        /// </summary>
        public string ExistingServiceName { get; set; } = string.Empty;
    }
}
