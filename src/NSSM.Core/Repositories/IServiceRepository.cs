using NSSM.Core.Services;

namespace NSSM.Core.Repositories.Interfaces
{
    /// <summary>
    /// Defines the contract for accessing and manipulating Windows service data.
    /// This repository interface abstracts the data access layer for service operations.
    /// </summary>
    public interface IServiceRepository
    {
        /// <summary>
        /// Retrieves all services from the system.
        /// </summary>
        /// <returns>A collection of service information objects</returns>
        Task<IEnumerable<ServiceInfo>> GetAllAsync();

        /// <summary>
        /// Retrieves a specific service by name.
        /// </summary>
        /// <param name="serviceName">The name of the service to retrieve</param>
        /// <returns>Service information if found, null otherwise</returns>
        Task<ServiceInfo?> GetByNameAsync( string serviceName );

        /// <summary>
        /// Creates a new service with the specified configuration.
        /// </summary>
        /// <param name="config">The service configuration</param>
        /// <returns>True if creation was successful, false otherwise</returns>
        Task<bool> CreateAsync( ServiceInstallConfig config );

        /// <summary>
        /// Updates an existing service with new configuration.
        /// </summary>
        /// <param name="config">The updated service configuration</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateAsync( ServiceUpdateConfig config );

        /// <summary>
        /// Deletes a service by name.
        /// </summary>
        /// <param name="serviceName">The name of the service to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync( string serviceName );

        /// <summary>
        /// Starts a service.
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if start was successful, false otherwise</returns>
        Task<bool> StartAsync( string serviceName, int? timeoutMs = null );

        /// <summary>
        /// Stops a service.
        /// </summary>
        /// <param name="serviceName">The name of the service to stop</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if stop was successful, false otherwise</returns>
        Task<bool> StopAsync( string serviceName, int? timeoutMs = null );
    }

}