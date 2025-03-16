using NSSM.Core.Models;

namespace NSSM.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for managing Windows services.
    /// This interface abstracts service operations to enable easier testing and replacement of implementations.
    /// </summary>
    public interface IWindowsServiceManager
    {
        /// <summary>
        /// Retrieves the last error encountered during service operations
        /// </summary>
        /// <returns>The service error information, or null if no error occurred</returns>
        Task<ServiceError?> GetLastErrorAsync();

        /// <summary>
        /// Retrieves a list of all installed services on the system.
        /// </summary>
        /// <returns>A collection of service information objects</returns>
        Task<IEnumerable<ServiceInfo>> GetAllServicesAsync();

        /// <summary>
        /// Installs a new Windows service with the specified configuration.
        /// </summary>
        /// <param name="config">The service configuration containing all necessary installation details</param>
        /// <returns>True if installation succeeds, otherwise false</returns>
        Task<bool> InstallServiceAsync( ServiceInstallConfig config );

        /// <summary>
        /// Removes an existing Windows service.
        /// </summary>
        /// <param name="serviceName">The name of the service to remove</param>
        /// <returns>True if removal succeeds, otherwise false</returns>
        Task<bool> RemoveServiceAsync( string serviceName );

        /// <summary>
        /// Starts a Windows service.
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if service started successfully, otherwise false</returns>
        Task<bool> StartServiceAsync(
            string serviceName,
            int? timeoutMs = null
        );

        /// <summary>
        /// Stops a Windows service.
        /// </summary>
        /// <param name="serviceName">The name of the service to stop</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if service stopped successfully, otherwise false</returns>
        Task<bool> StopServiceAsync(
            string serviceName,
            int? timeoutMs = null
        );

        /// <summary>
        /// Retrieves detailed information about a specific service.
        /// </summary>
        /// <param name="serviceName">The name of the service to query</param>
        /// <returns>A service information object if found, otherwise null</returns>
        Task<ServiceInfo?> GetServiceInfoAsync( string serviceName );

        /// <summary>
        /// Updates the configuration of an existing service.
        /// </summary>
        /// <param name="config">The updated service configuration</param>
        /// <returns>True if update succeeds, otherwise false</returns>
        Task<bool> UpdateServiceConfigAsync( ServiceUpdateConfig config );
    }
}
