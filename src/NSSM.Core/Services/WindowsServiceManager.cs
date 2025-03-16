using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using NSSM.Core.Models;
using NSSM.Core.Service;
using NSSM.Core.Service.Enums;
using NSSM.Core.Services.Interfaces;

namespace NSSM.Core.Services
{
    /// <summary>
    /// Implements the Windows service management interface, providing high-level operations for service management.
    /// This class acts as a facade over lower-level service management components.
    /// </summary>
    public class WindowsServiceManager : IWindowsServiceManager
    {
        private readonly ILogger<WindowsServiceManager> _logger;
        private readonly ServiceManager _serviceManager;
        private readonly ServiceRegistryManager _registryManager;

        /// <summary>
        /// Initializes a new instance of the WindowsServiceManager class
        /// </summary>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="serviceManager">The low-level service manager</param>
        /// <param name="registryManager">The registry manager for service configuration</param>
        public WindowsServiceManager(
            ILogger<WindowsServiceManager> logger,
            ServiceManager serviceManager,
            ServiceRegistryManager registryManager
        )
        {
            _logger = logger;
            _serviceManager = serviceManager;
            _registryManager = registryManager;
        }

        /// <summary>
        /// Retrieves the last error encountered during service operations
        /// </summary>
        /// <returns>The service error information, or null if no error occurred</returns>
        public async Task<ServiceError?> GetLastErrorAsync()
        {
            if( _serviceManager.LastErrorCode == 0 )
                return null;

            return new ServiceError
            {
                ErrorCode = _serviceManager.LastErrorCode,
                ErrorMessage = _serviceManager.LastErrorMessage
            };
        }

        /// <summary>
        /// Retrieves a list of all installed services on the system
        /// </summary>
        /// <returns>A collection of service information objects</returns>
        public async Task<IEnumerable<ServiceInfo>> GetAllServicesAsync()
        {
            var result = new List<ServiceInfo>();

            try
            {
                var serviceNames = await _registryManager.GetAllServiceNamesAsync();

                foreach( var name in serviceNames )
                {
                    var serviceInfo = await GetServiceInfoAsync( name );
                    if( serviceInfo != null )
                    {
                        result.Add( serviceInfo );
                    }
                }

                return result;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error retrieving all services" );
                return result;
            }
        }

        /// <summary>
        /// Installs a new Windows service with the specified configuration
        /// </summary>
        /// <param name="config">The service configuration containing all necessary installation details</param>
        /// <returns>True if installation succeeds, otherwise false</returns>
        public async Task<bool> InstallServiceAsync( ServiceInstallConfig config )
        {
            try
            {
                _logger.LogInformation( "Installing service {ServiceName}", config.ServiceName );

                //convert the install config to our complete configuration model
                var serviceConfig = new ServiceConfiguration
                {
                    ServiceName = config.ServiceName,
                    DisplayName = config.DisplayName,
                    Description = config.Description,
                    ExecutablePath = config.BinaryPath,
                    Arguments = config.Arguments,
                    Username = config.Account == "LocalSystem" ? null : config.Account,
                    StartupType = ConvertStartTypeFromString( config.StartType )
                };

                //install the service using the low-level service manager
                var success = _serviceManager.InstallService(
                    serviceConfig.ServiceName,
                    serviceConfig.DisplayName,
                    serviceConfig.Description,
                    serviceConfig.ExecutablePath,
                    serviceConfig.Arguments,
                    serviceConfig.StartupType,
                    serviceConfig.Username,
                    serviceConfig.Password );

                if( !success )
                {
                    _logger.LogError( "Failed to install service {ServiceName}: {ErrorMessage}",
                        config.ServiceName, _serviceManager.LastErrorMessage );
                    return false;
                }

                if( !await _registryManager.SaveNssmConfigurationAsync( serviceConfig ) )
                {
                    _logger.LogWarning( "Service installed but NSSM-specific configuration could not be saved" );
                }

                if( config.StartImmediately )
                {
                    return await StartServiceAsync( config.ServiceName );
                }

                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error installing service {ServiceName}", config.ServiceName );
                return false;
            }
        }

        /// <summary>
        /// Removes an existing Windows service
        /// </summary>
        /// <param name="serviceName">The name of the service to remove</param>
        /// <returns>True if removal succeeds, otherwise false</returns>
        public async Task<bool> RemoveServiceAsync( string serviceName )
        {
            try
            {
                _logger.LogInformation( "Removing service {ServiceName}", serviceName );

                //stop the service first if it's running
                var serviceInfo = await GetServiceInfoAsync( serviceName );
                if( serviceInfo != null && serviceInfo.Status == "Running" )
                {
                    await StopServiceAsync( serviceName );
                }

                bool success = _serviceManager.RemoveService( serviceName );

                if( !success )
                {
                    _logger.LogError( "Failed to remove service {ServiceName}: {ErrorMessage}",
                        serviceName, _serviceManager.LastErrorMessage );
                }

                return success;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error removing service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Starts a Windows service
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if service started successfully, otherwise false</returns>
        public async Task<bool> StartServiceAsync(
            string serviceName,
            int? timeoutMs = null
        )
        {
            try
            {
                _logger.LogInformation( "Starting service {ServiceName}", serviceName );

                using var serviceController = new ServiceController( serviceName );

                if( serviceController.Status == ServiceControllerStatus.Running )
                {
                    _logger.LogInformation( "Service {ServiceName} is already running", serviceName );
                    return true;
                }

                //use the timeout or default value
                var timeout = timeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds( timeoutMs.Value )
                    : TimeSpan.FromMilliseconds( 30000 ); // 30 seconds default

                //start the service
                serviceController.Start();
                serviceController.WaitForStatus( ServiceControllerStatus.Running, timeout );

                return serviceController.Status == ServiceControllerStatus.Running;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error starting service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Stops a Windows service
        /// </summary>
        /// <param name="serviceName">The name of the service to stop</param>
        /// <param name="timeoutMs">Optional timeout in milliseconds</param>
        /// <returns>True if service stopped successfully, otherwise false</returns>
        public async Task<bool> StopServiceAsync(
            string serviceName,
            int? timeoutMs = null
        )
        {
            try
            {
                _logger.LogInformation( "Stopping service {ServiceName}", serviceName );

                using var serviceController = new ServiceController( serviceName );

                if( serviceController.Status == ServiceControllerStatus.Stopped )
                {
                    _logger.LogInformation( "Service {ServiceName} is already stopped", serviceName );
                    return true;
                }

                //same as above, use the timeout or default value
                var timeout = timeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds( timeoutMs.Value )
                    : TimeSpan.FromMilliseconds( 30000 ); // 30 seconds default

                //stop the service
                serviceController.Stop();
                serviceController.WaitForStatus( ServiceControllerStatus.Stopped, timeout );

                return serviceController.Status == ServiceControllerStatus.Stopped;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error stopping service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific service
        /// </summary>
        /// <param name="serviceName">The name of the service to query</param>
        /// <returns>A service information object if found, otherwise null</returns>
        public async Task<ServiceInfo?> GetServiceInfoAsync( string serviceName )
        {
            try
            {
                _logger.LogDebug( "Getting information for service {ServiceName}", serviceName );

                var config = await _registryManager.GetServiceConfigurationAsync( serviceName );
                if( config == null )
                {
                    return null;
                }

                var status = string.Empty;
                try
                {
                    using var serviceController = new ServiceController( serviceName );
                    status = serviceController.Status.ToString();
                }
                catch
                {
                    status = "Unknown";
                }

                var serviceInfo = new ServiceInfo
                {
                    Name = config.ServiceName,
                    DisplayName = config.DisplayName,
                    Description = config.Description,
                    BinaryPath = config.ExecutablePath,
                    Status = status,
                    StartType = config.StartupType.ToString(),
                    Account = config.Username ?? "LocalSystem"
                };

                return serviceInfo;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error getting information for service {ServiceName}", serviceName );
                return null;
            }
        }

        /// <summary>
        /// Updates the configuration of an existing service
        /// </summary>
        /// <param name="config">The updated service configuration</param>
        /// <returns>True if update succeeds, otherwise false</returns>
        public async Task<bool> UpdateServiceConfigAsync( ServiceUpdateConfig config )
        {
            try
            {
                _logger.LogInformation( "Updating service configuration for {ServiceName}", config.ExistingServiceName );

                //first, check if we need to rename the service
                var renameRequired = !string.Equals( config.ExistingServiceName, config.ServiceName, StringComparison.OrdinalIgnoreCase );

                if( renameRequired )
                {
                    //windows doesn't support renaming services directly so remove and recreate with new name
                    _logger.LogWarning( "Service rename requested from {OldName} to {NewName}. Will remove and recreate.",
                        config.ExistingServiceName, config.ServiceName );

                    //get current service info before removing
                    var currentServiceInfo = await GetServiceInfoAsync( config.ExistingServiceName );
                    if( currentServiceInfo == null )
                    {
                        _logger.LogError( "Cannot find existing service {ServiceName} to rename", config.ExistingServiceName );
                        return false;
                    }

                    //remove the service
                    if( !await RemoveServiceAsync( config.ExistingServiceName ) )
                    {
                        _logger.LogError( "Failed to remove service {ServiceName} during rename operation", config.ExistingServiceName );
                        return false;
                    }

                    return await InstallServiceAsync( config );
                }
                else
                {
                    //standard update operation - get current config
                    var currentConfig = await _registryManager.GetServiceConfigurationAsync( config.ServiceName );
                    if( currentConfig == null )
                    {
                        _logger.LogError( "Cannot find existing service {ServiceName} to update", config.ServiceName );
                        return false;
                    }

                    //update with the new values
                    var serviceConfig = new ServiceConfiguration
                    {
                        ServiceName = config.ServiceName,
                        DisplayName = config.DisplayName,
                        Description = config.Description,
                        ExecutablePath = config.BinaryPath,
                        Arguments = config.Arguments,
                        Username = config.Account == "LocalSystem" ? null : config.Account,
                        Password = null, //password is not updated in this context
                        StartupType = ConvertStartTypeFromString( config.StartType )
                    };

                    //this is a simplified approach because ideally we would selectively update what's changed
                    var success = _serviceManager.UpdateService(
                        serviceConfig.ServiceName,
                        serviceConfig.DisplayName,
                        serviceConfig.Description,
                        serviceConfig.ExecutablePath,
                        serviceConfig.Arguments,
                        serviceConfig.StartupType );

                    if( !success )
                    {
                        _logger.LogError( "Failed to update service {ServiceName}: {ErrorMessage}",
                            config.ServiceName, _serviceManager.LastErrorMessage );
                        return false;
                    }

                    if( !await _registryManager.SaveNssmConfigurationAsync( serviceConfig ) )
                    {
                        _logger.LogWarning( "Service updated but NSSM-specific configuration could not be saved" );
                    }

                    return true;
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error updating service {ServiceName}", config.ServiceName );
                return false;
            }
        }

        /// <summary>
        /// Converts a string start type to the ServiceStartup enum
        /// </summary>
        /// <param name="startType">The start type as a string</param>
        /// <returns>The corresponding ServiceStartup enum value</returns>
        private ServiceStartup ConvertStartTypeFromString( string startType )
        {
            return startType.ToLowerInvariant() switch
            {
                "automatic" => ServiceStartup.AutoStart,
                "auto" => ServiceStartup.AutoStart,
                "delayed" => ServiceStartup.DelayedAutoStart,
                "demand" => ServiceStartup.DemandStart,
                "manual" => ServiceStartup.DemandStart,
                "disabled" => ServiceStartup.Disabled,
                _ => ServiceStartup.AutoStart
            };
        }
    }
}