using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;

namespace NSSM.Core.Factories
{
    /// <summary>
    /// Defines the contract for a factory that creates Windows service configurations.
    /// This factory centralizes the creation of service configurations with default values
    /// and validation logic.
    /// </summary>
    public interface IServiceFactory
    {
        /// <summary>
        /// Creates a new service installation configuration with default values.
        /// </summary>
        /// <returns>A service installation configuration with default values</returns>
        ServiceInstallConfig CreateDefaultInstallConfig();

        /// <summary>
        /// Creates a service update configuration based on an existing service.
        /// </summary>
        /// <param name="serviceName">The name of the existing service</param>
        /// <returns>A service update configuration populated with the current service settings</returns>
        Task<ServiceUpdateConfig?> CreateUpdateConfigFromExistingAsync( string serviceName );

        /// <summary>
        /// Validates a service installation configuration.
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <returns>True if the configuration is valid, otherwise false</returns>
        (bool IsValid, string[] ValidationErrors) ValidateInstallConfig( ServiceInstallConfig config );

        /// <summary>
        /// Validates a service update configuration.
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <returns>True if the configuration is valid, otherwise false</returns>
        (bool IsValid, string[] ValidationErrors) ValidateUpdateConfig( ServiceUpdateConfig config );
    }
}