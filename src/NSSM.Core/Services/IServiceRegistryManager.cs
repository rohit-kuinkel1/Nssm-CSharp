using NSSM.Core.Models;

namespace NSSM.Core.Services.Interfaces
{
    /// <summary>
    /// Defines operations for managing service registry configurations.
    /// Provides an abstraction over Windows registry operations for service settings.
    /// </summary>
    public interface IServiceRegistryManager
    {
        /// <summary>
        /// Retrieves a service configuration from the Windows registry
        /// </summary>
        /// <param name="serviceName">Name of the service to retrieve</param>
        /// <returns>The service configuration if found, null otherwise</returns>
        Task<ServiceConfiguration?> GetServiceConfigurationAsync( string serviceName );

        /// <summary>
        /// Saves NSSM-specific configuration to the registry
        /// </summary>
        /// <param name="config">Service configuration to save</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SaveNssmConfigurationAsync( ServiceConfiguration config );

        /// <summary>
        /// Retrieves a list of all services from the registry
        /// </summary>
        /// <returns>A collection of service names</returns>
        Task<IEnumerable<string>> GetAllServiceNamesAsync();
    }
}