using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NSSM.Core.Constants;
using NSSM.Core.Models;
using NSSM.Core.Services.Interfaces;
using System.IO;
using NSSM.Core.Service.Enums;

namespace NSSM.Core.Services
{
    /// <summary>
    /// Manages Windows registry operations for service configuration.
    /// This class encapsulates all registry access for services, providing a clean abstraction layer.
    /// </summary>
    public class ServiceRegistryManager : IServiceRegistryManager
    {
        private readonly ILogger<ServiceRegistryManager> _logger;
        
        /// <summary>
        /// Registry path for Windows services
        /// </summary>
        private const string ServicesRegistryPath = @"SYSTEM\CurrentControlSet\Services";
        
        /// <summary>
        /// Initializes a new instance of the ServiceRegistryManager class
        /// </summary>
        /// <param name="logger">Logger for diagnostic information</param>
        public ServiceRegistryManager(ILogger<ServiceRegistryManager> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Retrieves a service configuration from the Windows registry
        /// </summary>
        /// <param name="serviceName">Name of the service to retrieve</param>
        /// <returns>The service configuration if found, null otherwise</returns>
        public async Task<ServiceConfiguration?> GetServiceConfigurationAsync(string serviceName)
        {
            try
            {
                using var servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"{ServicesRegistryPath}\\{serviceName}");
                
                if (servicesKey == null)
                {
                    _logger.LogWarning("Service {ServiceName} not found in registry", serviceName);
                    return null;
                }
                
                var config = new ServiceConfiguration
                {
                    ServiceName = serviceName,
                    DisplayName = servicesKey.GetValue("DisplayName") as string ?? serviceName,
                    Description = servicesKey.GetValue("Description") as string ?? string.Empty,
                    ExecutablePath = ExtractExecutablePath(servicesKey.GetValue("ImagePath") as string ?? string.Empty),
                    Arguments = ExtractArguments(servicesKey.GetValue("ImagePath") as string ?? string.Empty),
                    StartupType = ConvertStartTypeToEnum(servicesKey.GetValue("Start") as int? ?? 2),
                    DelayedAutoStart = servicesKey.GetValue("DelayedAutostart") as int? == 1,
                    Username = servicesKey.GetValue("ObjectName") as string
                };
                
                // Get working directory if available
                using var parametersKey = servicesKey.OpenSubKey("Parameters");
                if (parametersKey != null)
                {
                    config.WorkingDirectory = parametersKey.GetValue("AppDirectory") as string ?? string.Empty;
                    config.LogDirectory = parametersKey.GetValue("AppStdout") as string;
                }
                
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service configuration for {ServiceName}", serviceName);
                return null;
            }
        }
        
        /// <summary>
        /// Saves NSSM-specific configuration to the registry
        /// </summary>
        /// <param name="config">Service configuration to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> SaveNssmConfigurationAsync(ServiceConfiguration config)
        {
            try
            {
                using var servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"{ServicesRegistryPath}\\{config.ServiceName}", true);
                
                if (servicesKey == null)
                {
                    _logger.LogWarning("Service {ServiceName} not found in registry", config.ServiceName);
                    return false;
                }
                
                // Create or open the Parameters subkey
                using var parametersKey = servicesKey.CreateSubKey("Parameters");
                
                // Set working directory if specified
                if (!string.IsNullOrEmpty(config.WorkingDirectory))
                {
                    parametersKey.SetValue("AppDirectory", config.WorkingDirectory);
                }
                
                // Set logging directory if specified
                if (!string.IsNullOrEmpty(config.LogDirectory))
                {
                    parametersKey.SetValue("AppStdout", config.LogDirectory);
                    parametersKey.SetValue("AppStderr", config.LogDirectory);
                }
                
                // Set exit action
                parametersKey.SetValue("AppExit", (int)config.OnExit);
                
                // Set process priority
                parametersKey.SetValue("AppPriority", (int)config.ProcessPriority);
                
                // Set throttle parameters
                parametersKey.SetValue("AppThrottle", config.RestartThrottleMs);
                
                // Set startup delay
                if (config.StartupDelayMs > 0)
                {
                    parametersKey.SetValue("AppStartupDelay", config.StartupDelayMs);
                }
                
                // Set rotation flag if enabled
                if (config.RotateLogsOnStart)
                {
                    parametersKey.SetValue("AppRotateFiles", 1);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving NSSM configuration for {ServiceName}", config.ServiceName);
                return false;
            }
        }
        
        /// <summary>
        /// Retrieves a list of all services from the registry
        /// </summary>
        /// <returns>A collection of service names</returns>
        public async Task<IEnumerable<string>> GetAllServiceNamesAsync()
        {
            var serviceNames = new List<string>();
            
            try
            {
                using var servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ServicesRegistryPath);
                
                if (servicesKey == null)
                {
                    _logger.LogError("Services registry key not found");
                    return serviceNames;
                }
                
                serviceNames.AddRange(servicesKey.GetSubKeyNames());
                return serviceNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service names from registry");
                return serviceNames;
            }
        }
        
        /// <summary>
        /// Extracts the executable path from the ImagePath registry value
        /// </summary>
        /// <param name="imagePath">The ImagePath value from the registry</param>
        /// <returns>The executable path without arguments</returns>
        private string ExtractExecutablePath(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;
            
            // Check if the path is quoted
            if (imagePath.StartsWith("\""))
            {
                var endQuoteIndex = imagePath.IndexOf("\"", 1);
                if (endQuoteIndex > 0)
                {
                    return imagePath.Substring(1, endQuoteIndex - 1);
                }
            }
            
            // If not quoted, split on first space
            var spaceIndex = imagePath.IndexOf(" ");
            if (spaceIndex > 0)
            {
                return imagePath.Substring(0, spaceIndex);
            }
            
            // No arguments
            return imagePath;
        }
        
        /// <summary>
        /// Extracts command-line arguments from the ImagePath registry value
        /// </summary>
        /// <param name="imagePath">The ImagePath value from the registry</param>
        /// <returns>The command-line arguments without the executable path</returns>
        private string ExtractArguments(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;
            
            // Check if the path is quoted
            if (imagePath.StartsWith("\""))
            {
                var endQuoteIndex = imagePath.IndexOf("\"", 1);
                if (endQuoteIndex > 0 && imagePath.Length > endQuoteIndex + 1)
                {
                    return imagePath.Substring(endQuoteIndex + 1).Trim();
                }
            }
            
            // If not quoted, split on first space
            var spaceIndex = imagePath.IndexOf(" ");
            if (spaceIndex > 0)
            {
                return imagePath.Substring(spaceIndex + 1).Trim();
            }
            
            // No arguments
            return string.Empty;
        }
        
        /// <summary>
        /// Converts a registry start type value to the ServiceStartup enum
        /// </summary>
        /// <param name="startType">The start type value from registry</param>
        /// <returns>The corresponding ServiceStartup enum value</returns>
        private ServiceStartup ConvertStartTypeToEnum(int startType)
        {
            return startType switch
            {
                2 => ServiceStartup.AutoStart,
                3 => ServiceStartup.DemandStart,
                4 => ServiceStartup.Disabled,
                _ => ServiceStartup.AutoStart
            };
        }

        public async Task<bool> DeleteServiceConfigurationAsync(string serviceName)
        {
            try
            {
                // Try to delete the service configuration from the registry
                using (var servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ServicesRegistryPath, true))
                {
                    if (servicesKey == null)
                    {
                        _logger.LogWarning("Services registry key not found");
                        return false;
                    }
                    
                    servicesKey.DeleteSubKeyTree(serviceName, false);
                    _logger.LogInformation("Deleted service configuration for {ServiceName}", serviceName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service configuration for {ServiceName}", serviceName);
                return false;
            }
        }

        public async Task<bool> UpdateServiceConfigurationAsync(ServiceConfiguration config)
        {
            try
            {
                // Update service configuration in the registry
                using (var servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"{ServicesRegistryPath}\\{config.ServiceName}", true))
                {
                    if (servicesKey == null)
                    {
                        _logger.LogWarning("Service {ServiceName} not found in registry", config.ServiceName);
                        return false;
                    }
                    
                    // Update executable path and arguments in Parameters subkey
                    using (var parametersKey = servicesKey.CreateSubKey("Parameters"))
                    {
                        if (parametersKey != null)
                        {
                            parametersKey.SetValue("Application", config.ExecutablePath);
                            parametersKey.SetValue("AppParameters", config.Arguments ?? string.Empty);
                            
                            if (!string.IsNullOrEmpty(config.WorkingDirectory))
                            {
                                parametersKey.SetValue("AppDirectory", config.WorkingDirectory);
                            }
                            
                            if (!string.IsNullOrEmpty(config.LogDirectory))
                            {
                                parametersKey.SetValue("AppStdoutPath", Path.Combine(config.LogDirectory, $"{config.ServiceName}_stdout.log"));
                                parametersKey.SetValue("AppStderrPath", Path.Combine(config.LogDirectory, $"{config.ServiceName}_stderr.log"));
                            }
                        }
                    }
                    
                    _logger.LogInformation("Updated service configuration for {ServiceName}", config.ServiceName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service configuration for {ServiceName}", config.ServiceName);
                return false;
            }
        }
    }
} 