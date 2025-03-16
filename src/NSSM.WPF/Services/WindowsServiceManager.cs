using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using NSSM.Core.Service;
using NSSM.Core.Service.Enums;
using NSSM.Core.Services;

namespace NSSM.WPF.Services
{
    public class WindowsServiceManager : NSSM.Core.Services.Interfaces.IWindowsServiceManager
    {
        private readonly ServiceManager _coreServiceManager;
        private NSSM.Core.Models.ServiceError? _lastError;

        public WindowsServiceManager( ServiceManager coreServiceManager )
        {
            _coreServiceManager = coreServiceManager;
        }

        /// <summary>
        /// Gets the last error encountered during service operations
        /// </summary>
        public Task<NSSM.Core.Models.ServiceError?> GetLastErrorAsync()
        {
            return Task.FromResult( _lastError );
        }

        public bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal( identity );
            return principal.IsInRole( WindowsBuiltInRole.Administrator );
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ServiceInfo>> GetAllServicesAsync()
        {
            var services = new List<ServiceInfo>();

            try
            {
                //use WMI to get full service details including descriptions
                var searcher = new ManagementObjectSearcher( "SELECT * FROM Win32_Service" );
                var serviceWmiObjects = searcher.Get();

                //create a dictionary of services with their descriptions from WMI
                var serviceDescriptions = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
                var serviceAccounts = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
                var serviceStartModes = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
                var servicePathNames = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

                foreach( ManagementObject service in serviceWmiObjects )
                {
                    string name = service["Name"]?.ToString() ?? string.Empty;
                    if( !string.IsNullOrEmpty( name ) )
                    {
                        serviceDescriptions[name] = service["Description"]?.ToString() ?? string.Empty;
                        serviceAccounts[name] = service["StartName"]?.ToString() ?? "LocalSystem";
                        serviceStartModes[name] = service["StartMode"]?.ToString() ?? "Unknown";
                        servicePathNames[name] = service["PathName"]?.ToString() ?? string.Empty;
                    }
                }

                //now get service status using ServiceController
                var serviceControllers = ServiceController.GetServices();
                foreach( var sc in serviceControllers )
                {
                    try
                    {
                        //get description from the WMI data
                        serviceDescriptions.TryGetValue( sc.ServiceName, out string? description );
                        serviceAccounts.TryGetValue( sc.ServiceName, out string? account );
                        serviceStartModes.TryGetValue( sc.ServiceName, out string? startType );
                        servicePathNames.TryGetValue( sc.ServiceName, out string? binaryPath );

                        //ensure we have non-null values
                        description ??= string.Empty;
                        account ??= "LocalSystem";
                        startType ??= "Unknown";
                        binaryPath ??= string.Empty;

                        // Map start mode to the format Windows displays
                        string formattedStartType = startType switch
                        {
                            "Auto" => "Automatic",
                            "Manual" => "Manual",
                            "Disabled" => "Disabled",
                            _ => startType
                        };

                        services.Add( new ServiceInfo
                        {
                            Name = sc.ServiceName,
                            DisplayName = sc.DisplayName,
                            Status = sc.Status.ToString(),
                            Description = description,
                            StartType = formattedStartType,
                            Account = account,
                            BinaryPath = binaryPath
                        } );
                    }
                    catch
                    {
                        //if we fail to get WMI data for a service, add basic info only
                        services.Add( new ServiceInfo
                        {
                            Name = sc.ServiceName,
                            DisplayName = sc.DisplayName,
                            Status = sc.Status.ToString()
                        } );
                    }
                }
            }
            catch( Win32Exception ex )
            {
                if( ex.NativeErrorCode == 5 ) //access denied
                {
                    Console.WriteLine( $"Insufficient permissions to retrieve all services: {ex.Message}" );
                }
                else
                {
                    Console.WriteLine( $"Error getting services: {ex.Message}" );
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine( $"Error getting services: {ex.Message}" );
            }

            return services;
        }

        /// <inheritdoc/>
        public async Task<bool> StartServiceAsync( string serviceName, int? timeoutMs = null )
        {
            if( !IsAdministrator() )
            {
                return false;
            }

            try
            {
                using var sc = new ServiceController( serviceName );
                if( sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending )
                {
                    sc.Start();
                    await WaitForStatusAsync( sc, ServiceControllerStatus.Running,
                        timeoutMs.HasValue ? TimeSpan.FromMilliseconds( timeoutMs.Value ) : TimeSpan.FromSeconds( 30 ) );
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StopServiceAsync( string serviceName, int? timeoutMs = null )
        {
            if( !IsAdministrator() )
            {
                return false;
            }

            try
            {
                using var sc = new ServiceController( serviceName );
                if( sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending )
                {
                    sc.Stop();
                    await WaitForStatusAsync( sc, ServiceControllerStatus.Stopped,
                        timeoutMs.HasValue ? TimeSpan.FromMilliseconds( timeoutMs.Value ) : TimeSpan.FromSeconds( 30 ) );
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> InstallServiceAsync( ServiceInstallConfig config )
        {
            _lastError = null;

            if( !IsAdministrator() )
            {
                System.Diagnostics.Debug.WriteLine( "Cannot install service: Administrator privileges required" );
                _lastError = new NSSM.Core.Models.ServiceError
                {
                    ErrorCode = 5, // ERROR_ACCESS_DENIED
                    ErrorMessage = "Administrator privileges required"
                };
                return false;
            }

            try
            {
                if( string.IsNullOrWhiteSpace( config.ServiceName ) )
                {
                    System.Diagnostics.Debug.WriteLine( "Cannot install service: Service name is empty" );
                    _lastError = new NSSM.Core.Models.ServiceError
                    {
                        ErrorCode = 87, // ERROR_INVALID_PARAMETER
                        ErrorMessage = "Service name cannot be empty"
                    };
                    return false;
                }

                if( string.IsNullOrWhiteSpace( config.BinaryPath ) )
                {
                    System.Diagnostics.Debug.WriteLine( "Cannot install service: Binary path is empty" );
                    _lastError = new NSSM.Core.Models.ServiceError
                    {
                        ErrorCode = 87, // ERROR_INVALID_PARAMETER
                        ErrorMessage = "Binary path cannot be empty"
                    };
                    return false;
                }

                //check if service exists already
                try
                {
                    using var sc = new ServiceController( config.ServiceName );
                    if( sc.ServiceName == config.ServiceName )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Cannot install service: Service '{config.ServiceName}' already exists" );
                        _lastError = new NSSM.Core.Models.ServiceError
                        {
                            ErrorCode = 1073, // ERROR_SERVICE_EXISTS
                            ErrorMessage = $"Service '{config.ServiceName}' already exists"
                        };
                        return false;
                    }
                }
                catch( InvalidOperationException )
                {
                    //service doesn't exist, which is what we want
                    System.Diagnostics.Debug.WriteLine( $"Service '{config.ServiceName}' does not exist yet - proceeding with installation" );
                }

                var result = false;
                await Task.Run( () =>
                {
                    System.Diagnostics.Debug.WriteLine( $"Installing service: {config.ServiceName} - {config.BinaryPath} {config.Arguments}" );
                    System.Diagnostics.Debug.WriteLine( $"Service account: {config.Account}" );

                    //localSystem is a special account in Windows services
                    string? accountName = null;
                    if( !string.IsNullOrEmpty( config.Account ) && config.Account != "LocalSystem" )
                    {
                        accountName = config.Account;
                        System.Diagnostics.Debug.WriteLine( $"Using custom account: {accountName}" );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( "Using LocalSystem account" );
                    }

                    try
                    {
                        result = _coreServiceManager.InstallService(
                            config.ServiceName,
                            config.DisplayName,
                            config.Description,
                            config.BinaryPath,
                            config.Arguments,
                            ServiceStartup.AutoStart,
                            accountName,  //null for LocalSystem, otherwise use the specified account
                            null );  //no password for now

                        if( !result )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service installation failed for {config.ServiceName} - the executable may not be suitable to run as a service" );

                            var errorCode = _coreServiceManager.LastErrorCode;
                            var errorMessage = _coreServiceManager.LastErrorMessage;

                            if( errorCode != 0 )
                            {
                                System.Diagnostics.Debug.WriteLine( $"Service installation failed with error code {errorCode}: {errorMessage}" );

                                _lastError = new NSSM.Core.Models.ServiceError
                                {
                                    ErrorCode = errorCode,
                                    ErrorMessage = errorMessage
                                };
                            }
                            else if( _lastError == null )
                            {
                                _lastError = new NSSM.Core.Models.ServiceError
                                {
                                    ErrorCode = 1, //generic error 
                                    ErrorMessage = "Service installation failed. The core service manager reported a failure."
                                };
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service installation succeeded for {config.ServiceName}" );
                        }
                    }
                    catch( System.ComponentModel.Win32Exception w32Ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Win32 Error installing service: {w32Ex.NativeErrorCode} - {w32Ex.Message}" );
                        _lastError = new NSSM.Core.Models.ServiceError
                        {
                            ErrorCode = w32Ex.NativeErrorCode,
                            ErrorMessage = w32Ex.Message
                        };
                        result = false;
                    }
                    catch( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Error during service installation: {ex.Message}" );
                        System.Diagnostics.Debug.WriteLine( $"Exception details: {ex}" );

                        var win32Exception = ex.InnerException as Win32Exception;
                        if( win32Exception != null )
                        {
                            _lastError = new NSSM.Core.Models.ServiceError
                            {
                                ErrorCode = win32Exception.NativeErrorCode,
                                ErrorMessage = win32Exception.Message
                            };
                        }
                        else
                        {
                            _lastError = new NSSM.Core.Models.ServiceError
                            {
                                ErrorCode = Marshal.GetLastWin32Error(),
                                ErrorMessage = ex.Message
                            };
                        }

                        result = false;
                    }

                    System.Diagnostics.Debug.WriteLine( $"Install service result: {result}" );
                } );

                //double-check if service exists now
                if( result )
                {
                    bool verified = false;

                    try
                    {
                        using var sc = new ServiceController( config.ServiceName );
                        if( sc.ServiceName == config.ServiceName )
                        {
                            verified = true;
                            System.Diagnostics.Debug.WriteLine( $"Service verification successful (ServiceController): {config.ServiceName} found after installation" );
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service verification failed (ServiceController): {config.ServiceName} not found after installation" );
                        }
                    }
                    catch( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Service verification error (ServiceController): {ex.Message}" );
                    }

                    //try to get the service details from WMI to verify it's actually using our executable
                    try
                    {
                        string query = $"SELECT * FROM Win32_Service WHERE Name = '{config.ServiceName}'";
                        var searcher = new ManagementObjectSearcher( query );
                        var collection = searcher.Get();

                        bool foundInWmi = false;
                        foreach( ManagementObject service in collection )
                        {
                            foundInWmi = true;
                            string pathName = service["PathName"]?.ToString() ?? string.Empty;
                            string state = service["State"]?.ToString() ?? string.Empty;
                            string startMode = service["StartMode"]?.ToString() ?? string.Empty;

                            System.Diagnostics.Debug.WriteLine( $"Service WMI verification: Found service '{config.ServiceName}'" );
                            System.Diagnostics.Debug.WriteLine( $"  - Path: {pathName}" );
                            System.Diagnostics.Debug.WriteLine( $"  - State: {state}" );
                            System.Diagnostics.Debug.WriteLine( $"  - Start Mode: {startMode}" );

                            verified = true;
                            break;
                        }

                        if( !foundInWmi )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service verification failed (WMI): {config.ServiceName} not found in WMI query" );
                        }
                    }
                    catch( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Error getting service details from WMI: {ex.Message}" );
                    }

                    //as a last resort, check for the registry key
                    try
                    {
                        using var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            $"SYSTEM\\CurrentControlSet\\Services\\{config.ServiceName}" );

                        if( serviceKey != null )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service verification successful (Registry): Found registry key for service {config.ServiceName}" );
                            verified = true;

                            //check the ImagePath value to see what executable is set
                            var imagePath = serviceKey.GetValue( "ImagePath" )?.ToString() ?? string.Empty;
                            System.Diagnostics.Debug.WriteLine( $"  - ImagePath: {imagePath}" );

                            //check if parameters subkey exists
                            using var paramsKey = serviceKey.OpenSubKey( "Parameters" );
                            if( paramsKey != null )
                            {
                                System.Diagnostics.Debug.WriteLine( $"  - Parameters key exists" );
                                var app = paramsKey.GetValue( "Application" )?.ToString() ?? string.Empty;
                                var appParams = paramsKey.GetValue( "AppParameters" )?.ToString() ?? string.Empty;
                                System.Diagnostics.Debug.WriteLine( $"  - Application: {app}" );
                                System.Diagnostics.Debug.WriteLine( $"  - AppParameters: {appParams}" );
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine( $"  - Parameters key does not exist" );
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine( $"Service verification failed (Registry): No registry key found for service {config.ServiceName}" );
                        }
                    }
                    catch( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Error checking registry for service: {ex.Message}" );
                    }

                    return verified;
                }

                return result;
            }
            catch( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error installing service: {ex.Message}" );
                System.Diagnostics.Debug.WriteLine( $"Exception: {ex}" );
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveServiceAsync( string serviceName )
        {
            if( !IsAdministrator() )
            {
                return false;
            }

            try
            {
                await Task.Run( () =>
                {
                    _coreServiceManager.RemoveService( serviceName );
                } );
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceInfo?> GetServiceInfoAsync( string serviceName )
        {
            try
            {
                using var sc = new ServiceController( serviceName );

                string query = $"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'";
                var searcher = new ManagementObjectSearcher( query );
                var collection = searcher.Get();

                string description = string.Empty;
                string account = "LocalSystem";
                string startType = "Unknown";
                string binaryPath = string.Empty;

                foreach( ManagementObject service in collection )
                {
                    description = service["Description"]?.ToString() ?? string.Empty;
                    account = service["StartName"]?.ToString() ?? "LocalSystem";
                    startType = service["StartMode"]?.ToString() ?? "Unknown";
                    binaryPath = service["PathName"]?.ToString() ?? string.Empty;
                    break; //we only expect one result
                }

                //map start mode to the format Windows displays
                var formattedStartType = startType switch
                {
                    "Auto" => "Automatic",
                    "Manual" => "Manual",
                    "Disabled" => "Disabled",
                    _ => startType
                };

                return new ServiceInfo
                {
                    Name = sc.ServiceName,
                    DisplayName = sc.DisplayName,
                    Status = sc.Status.ToString(),
                    Description = description,
                    StartType = formattedStartType,
                    Account = account,
                    BinaryPath = binaryPath
                };
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateServiceConfigAsync( ServiceUpdateConfig config )
        {
            //implementation would need to use Win32 API to update service config; just return false for now to indicate that the update was not done
            return false;
        }

        private static async Task WaitForStatusAsync( ServiceController sc, ServiceControllerStatus status, TimeSpan timeout )
        {
            await Task.Run( () =>
            {
                sc.WaitForStatus( status, timeout );
            } );
        }
    }
}