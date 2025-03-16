using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSSM.Core.Configuration;
using NSSM.Core.Constants;
using NSSM.Core.Exceptions;
using NSSM.Core.Logging;
using NSSM.Core.Repositories.Interfaces;
using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSSM.Core.Repositories;

/// <summary>
/// Implementation of the service repository that provides access to Windows services.
/// This repository uses the IWindowsServiceManager to perform actual service operations
/// and adds error handling, logging, and configuration.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly IWindowsServiceManager _serviceManager;
    private readonly LoggingService _loggingService;
    private readonly ServiceManagerOptions _options;

    /// <summary>
    /// Initializes a new instance of the ServiceRepository class.
    /// </summary>
    /// <param name="serviceManager">The service manager to use for service operations</param>
    /// <param name="loggingService">The logging service for recording operations</param>
    /// <param name="options">The service manager configuration options</param>
    public ServiceRepository(
        IWindowsServiceManager serviceManager,
        LoggingService loggingService,
        IOptions<ServiceManagerOptions> options)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Retrieves all services from the system.
    /// </summary>
    /// <returns>A collection of service information objects</returns>
    public async Task<IEnumerable<ServiceInfo>> GetAllAsync()
    {
        try
        {
            _loggingService.LogInformation("Retrieving all services");
            return await _serviceManager.GetAllServicesAsync();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Failed to retrieve all services", ex);
            throw new NssmException(ErrorCode.Unexpected, "Failed to retrieve the list of services", ex);
        }
    }

    /// <summary>
    /// Retrieves a specific service by name.
    /// </summary>
    /// <param name="serviceName">The name of the service to retrieve</param>
    /// <returns>Service information if found, null otherwise</returns>
    public async Task<ServiceInfo?> GetByNameAsync(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        }

        try
        {
            _loggingService.LogInformation($"Retrieving service '{serviceName}'");
            return await _serviceManager.GetServiceInfoAsync(serviceName);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to retrieve service '{serviceName}'", ex);
            throw new NssmException(ErrorCode.Unexpected, $"Failed to retrieve information for service '{serviceName}'", ex);
        }
    }

    /// <summary>
    /// Creates a new service with the specified configuration.
    /// </summary>
    /// <param name="config">The service configuration</param>
    /// <returns>True if creation was successful, false otherwise</returns>
    public async Task<bool> CreateAsync(ServiceInstallConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.ServiceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(config));
        }

        try
        {
            _loggingService.LogInformation($"Creating service '{config.ServiceName}'");
            return await _serviceManager.InstallServiceAsync(config);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to create service '{config.ServiceName}'", ex);
            throw new NssmException(ErrorCode.ServiceInstallFailure, $"Failed to create service '{config.ServiceName}'", ex);
        }
    }

    /// <summary>
    /// Updates an existing service with new configuration.
    /// </summary>
    /// <param name="config">The updated service configuration</param>
    /// <returns>True if update was successful, false otherwise</returns>
    public async Task<bool> UpdateAsync(ServiceUpdateConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.ExistingServiceName))
        {
            throw new ArgumentException("Existing service name cannot be null or empty", nameof(config));
        }

        try
        {
            _loggingService.LogInformation($"Updating service '{config.ExistingServiceName}'");
            return await _serviceManager.UpdateServiceConfigAsync(config);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to update service '{config.ExistingServiceName}'", ex);
            throw new NssmException(ErrorCode.Unexpected, $"Failed to update service '{config.ExistingServiceName}'", ex);
        }
    }

    /// <summary>
    /// Deletes a service by name.
    /// </summary>
    /// <param name="serviceName">The name of the service to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    public async Task<bool> DeleteAsync(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        }

        try
        {
            _loggingService.LogInformation($"Deleting service '{serviceName}'");
            return await _serviceManager.RemoveServiceAsync(serviceName);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to delete service '{serviceName}'", ex);
            throw new NssmException(ErrorCode.ServiceRemovalFailure, $"Failed to delete service '{serviceName}'", ex);
        }
    }

    /// <summary>
    /// Starts a service.
    /// </summary>
    /// <param name="serviceName">The name of the service to start</param>
    /// <param name="timeoutMs">Optional timeout in milliseconds</param>
    /// <returns>True if start was successful, false otherwise</returns>
    public async Task<bool> StartAsync(string serviceName, int? timeoutMs = null)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        }

        try
        {
            int timeout = timeoutMs ?? _options.DefaultTimeoutMs;
            _loggingService.LogInformation($"Starting service '{serviceName}' with timeout of {timeout}ms");
            return await _serviceManager.StartServiceAsync(serviceName, timeout);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to start service '{serviceName}'", ex);
            throw new NssmException(ErrorCode.ServiceStartFailure, $"Failed to start service '{serviceName}'", ex);
        }
    }

    /// <summary>
    /// Stops a service.
    /// </summary>
    /// <param name="serviceName">The name of the service to stop</param>
    /// <param name="timeoutMs">Optional timeout in milliseconds</param>
    /// <returns>True if stop was successful, false otherwise</returns>
    public async Task<bool> StopAsync(string serviceName, int? timeoutMs = null)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        }

        try
        {
            int timeout = timeoutMs ?? _options.DefaultTimeoutMs;
            _loggingService.LogInformation($"Stopping service '{serviceName}' with timeout of {timeout}ms");
            return await _serviceManager.StopServiceAsync(serviceName, timeout);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to stop service '{serviceName}'", ex);
            throw new NssmException(ErrorCode.ServiceStopFailure, $"Failed to stop service '{serviceName}'", ex);
        }
    }
}
