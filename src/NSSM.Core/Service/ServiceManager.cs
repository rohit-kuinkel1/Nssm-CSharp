using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NSSM.Core.Service.Enums;

namespace NSSM.Core.Service
{

    /// <summary>
    /// Core service management functionality for NSSM
    /// This is a C# implementation of the functionality in service.cpp
    /// </summary>
    public class ServiceManager
    {
        private readonly ILogger<ServiceManager> _logger;
        private readonly INativeMethods _nativeMethods;

        //store the last error that occurred
        private int _lastErrorCode;
        private string _lastErrorMessage = string.Empty;

        //constants from the original C++ code
        public const int ServiceNameLength = 256;
        public const int ExeLength = 260; // PATH_LENGTH in C++
        public const int CmdLength = 32768;
        public const int KeyLength = 255;
        public const int ValueLength = 16383;
        public const int DirLength = 248; // DIR_LENGTH in C++

        //service status constants
        public const int ServiceControlStart = 0x00000000;
        public const int ServiceControlStop = 0x00000001;
        public const int ServiceControlPause = 0x00000002;
        public const int ServiceControlContinue = 0x00000003;
        public const int ServiceControlInterrogate = 0x00000004;
        public const int ServiceControlShutdown = 0x00000005;
        public const int ServiceControlRotate = 0x00000006;

        //default values
        public const int DefaultResetThrottleRestart = 1500;
        public const int DefaultKillConsoleDelay = 1500;
        public const int DefaultKillWindowDelay = 1500;
        public const int DefaultKillThreadsDelay = 1000;
        public const int DefaultStartupDelay = 0;

        public ServiceManager( ILogger<ServiceManager> logger )
            : this( logger, new WindowsNativeMethods() )
        {
        }

        public ServiceManager(
            ILogger<ServiceManager> logger,
            INativeMethods nativeMethods
        )
        {
            _logger = logger;
            _nativeMethods = nativeMethods;
        }

        /// <summary>
        /// Gets the last Win32 error code encountered during service operations
        /// </summary>
        public int LastErrorCode => _lastErrorCode;

        /// <summary>
        /// Gets the last error message encountered during service operations
        /// </summary>
        public string LastErrorMessage => _lastErrorMessage;

        /// <summary>
        /// Resets the last error information
        /// </summary>
        public void ResetLastError()
        {
            _lastErrorCode = 0;
            _lastErrorMessage = string.Empty;
        }

        /// <summary>
        /// Sets the last error information
        /// </summary>
        private void SetLastError(
            int errorCode,
            string? message = null
        )
        {
            _lastErrorCode = errorCode;
            _lastErrorMessage = message ?? new System.ComponentModel.Win32Exception( errorCode ).Message;

            _logger.LogDebug( "Set last error: {ErrorCode} - {ErrorMessage}", _lastErrorCode, _lastErrorMessage );
        }

        /// <summary>
        /// Installs a new Windows service
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="displayName">Display name of the service</param>
        /// <param name="description">Description of the service</param>
        /// <param name="executablePath">Path to the executable</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="startupType">Service startup type</param>
        /// <param name="username">Username to run the service as</param>
        /// <param name="password">Password for the username</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool InstallService(
            string serviceName,
            string displayName,
            string description,
            string executablePath,
            string arguments,
            ServiceStartup startupType = ServiceStartup.AutoStart,
            string? username = null,
            string? password = null
        )
        {
            try
            {
                ResetLastError();

                //convert our startup type to ServiceStartMode
                var startMode = startupType switch
                {
                    ServiceStartup.AutoStart => ServiceStartMode.Automatic,
                    ServiceStartup.DelayedAutoStart => ServiceStartMode.Automatic, //we'll set delayed flag separately
                    ServiceStartup.DemandStart => ServiceStartMode.Manual,
                    ServiceStartup.Disabled => ServiceStartMode.Disabled,
                    _ => ServiceStartMode.Automatic
                };

                var scmHandle = _nativeMethods.OpenSCManager( null, null, NativeMethods.SC_MANAGER_ALL_ACCESS );
                if( scmHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                    _logger.LogError( "Failed to open service control manager: Error {ErrorCode} - {ErrorMessage}",
                        error, errorMessage );
                    SetLastError( error, errorMessage );
                    return false;
                }

                //instead of registering the application directly as a service,
                // we need to register our own NSSM executable as the service because registering exes as a 
                //service doesnt always work so NSSM will work like a wrapper

                var nssmExecutablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                    ?? System.Reflection.Assembly.GetExecutingAssembly().Location;

                //make sure we're using the CLI executable, not the WPF executable
                if( nssmExecutablePath.Contains( "NSSM.WPF" ) )
                {
                    string directory = Path.GetDirectoryName( nssmExecutablePath ) ?? string.Empty;
                    string cliExePath = Path.Combine( directory, "NSSM.CLI.exe" );

                    if( File.Exists( cliExePath ) )
                    {
                        nssmExecutablePath = cliExePath;
                        _logger.LogDebug( "Switching to CLI executable for service host: {NssmPath}", nssmExecutablePath );
                    }
                }

                _logger.LogDebug( "Using NSSM executable as service host: {NssmPath}", nssmExecutablePath );

                //path should be in quotes if it contains spaces
                string formattedPath = nssmExecutablePath;
                if( formattedPath.Contains( " " ) )
                {
                    formattedPath = $"\"{formattedPath}\"";
                }

                //ensure service name is also quoted if it contains spaces before passing it
                string quotedServiceName = serviceName;
                if( quotedServiceName.Contains( " " ) )
                {
                    quotedServiceName = $"\"{quotedServiceName}\"";
                }

                string binaryPath = $"{formattedPath} --run-as-service {quotedServiceName}";

                _logger.LogDebug( "Creating service with binary path: {BinaryPath}", binaryPath );

                //localSys account is represented by "NT AUTHORITY\\LocalSystem" or null in Windows APIs
                //empty string is interpreted as a custom account, which requires a password
                string serviceAccount = string.IsNullOrEmpty( username ) ? "LocalSystem" : username;

                _logger.LogDebug( "Creating service with account: {Account}", serviceAccount );

                var serviceHandle = _nativeMethods.CreateService(
                    scmHandle,
                    serviceName,
                    displayName,
                    NativeMethods.SERVICE_ALL_ACCESS,
                    NativeMethods.SERVICE_WIN32_OWN_PROCESS,
                    (uint)startMode,
                    NativeMethods.SERVICE_ERROR_NORMAL,
                    binaryPath,
                    string.Empty,  // lpLoadOrderGroup
                    IntPtr.Zero,
                    string.Empty,  // lpDependencies
                    string.IsNullOrEmpty( username ) ? "LocalSystem" : username,
                    password ?? string.Empty
                );

                if( serviceHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                    _logger.LogError( "Failed to create service {ServiceName}: Error {ErrorCode} - {ErrorMessage}",
                        serviceName, error, errorMessage );
                    _ = _nativeMethods.CloseServiceHandle( scmHandle );
                    SetLastError( error, errorMessage );
                    return false;
                }

                //ensure handles are properly closed when we're done
                _ = _nativeMethods.CloseServiceHandle( serviceHandle );
                _ = _nativeMethods.CloseServiceHandle( scmHandle );

                //set delayed auto start if needed
                if( startupType == ServiceStartup.DelayedAutoStart )
                {
                    try
                    {
                        using var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            $"SYSTEM\\CurrentControlSet\\Services\\{serviceName}", true );
                        if( serviceKey != null )
                        {
                            serviceKey.SetValue( "DelayedAutostart", 1, Microsoft.Win32.RegistryValueKind.DWord );
                        }
                        else
                        {
                            _logger.LogWarning( "Could not open registry key for service {ServiceName} to set delayed start", serviceName );
                        }
                    }
                    catch( Exception ex )
                    {
                        _logger.LogError( ex, "Error setting delayed auto start for service {ServiceName}", serviceName );
                        //continue anyway, this is not critical
                    }
                }

                //set description
                if( !string.IsNullOrEmpty( description ) )
                {
                    try
                    {
                        using var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            $"SYSTEM\\CurrentControlSet\\Services\\{serviceName}", true );
                        if( serviceKey != null )
                        {
                            serviceKey.SetValue( "Description", description );
                        }
                        else
                        {
                            _logger.LogWarning( "Could not open registry key for service {ServiceName} to set description", serviceName );
                        }
                    }
                    catch( Exception ex )
                    {
                        _logger.LogError( ex, "Error setting description for service {ServiceName}", serviceName );
                        //continue anyway, this is not critical
                    }
                }

                try
                {
                    //this is where we store the ACTUAL application's path
                    CreateParameters( serviceName, executablePath, arguments );
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, "Error creating parameters for service {ServiceName}", serviceName );
                    //not critical for the service to function, so continue
                }

                _logger.LogInformation( "Successfully installed service {ServiceName}", serviceName );

                //verify the service exists as a final check
                try
                {
                    var scmHandleCheck = _nativeMethods.OpenSCManager( null, null, NativeMethods.SC_MANAGER_ALL_ACCESS );
                    if( scmHandleCheck != IntPtr.Zero )
                    {
                        var serviceHandleCheck = _nativeMethods.OpenService(
                            scmHandleCheck, serviceName, NativeMethods.SERVICE_QUERY_STATUS );

                        bool serviceExists = serviceHandleCheck != IntPtr.Zero;

                        if( serviceHandleCheck != IntPtr.Zero )
                        {
                            _ = _nativeMethods.CloseServiceHandle( serviceHandleCheck );
                        }

                        _ = _nativeMethods.CloseServiceHandle( scmHandleCheck );

                        if( !serviceExists )
                        {
                            _logger.LogError( "Service installation reported success but service {ServiceName} does not exist", serviceName );
                            return false;
                        }
                    }
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, "Error verifying service {ServiceName} installation", serviceName );
                    //continue anyway, we'll report success based on the installation steps
                }

                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error installing service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Removes a Windows service
        /// </summary>
        /// <param name="serviceName">Name of the service to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RemoveService( string serviceName )
        {
            try
            {
                //open the scm
                var scmHandle = _nativeMethods.OpenSCManager( null, null, NativeMethods.SC_MANAGER_ALL_ACCESS );
                if( scmHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    _logger.LogError( "Failed to open service control manager: {Error}", error );
                    return false;
                }

                //open the service
                var serviceHandle = _nativeMethods.OpenService(
                    scmHandle,
                    serviceName,
                    NativeMethods.SERVICE_ALL_ACCESS );

                if( serviceHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    _logger.LogError( "Failed to open service {ServiceName}: {Error}", serviceName, error );
                    _nativeMethods.CloseServiceHandle( scmHandle );
                    return false;
                }

                //delete service
                if( !_nativeMethods.DeleteService( serviceHandle ) )
                {
                    var error = Marshal.GetLastWin32Error();
                    _logger.LogError( "Failed to delete service {ServiceName}: {Error}", serviceName, error );
                    _nativeMethods.CloseServiceHandle( serviceHandle );
                    _nativeMethods.CloseServiceHandle( scmHandle );
                    return false;
                }

                _nativeMethods.CloseServiceHandle( serviceHandle );
                _nativeMethods.CloseServiceHandle( scmHandle );

                //stop the service if it's running
                try
                {
                    var serviceController = new ServiceController( serviceName );
                    if( serviceController.Status != ServiceControllerStatus.Stopped )
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus( ServiceControllerStatus.Stopped, TimeSpan.FromSeconds( 30 ) );
                    }
                }
                catch( InvalidOperationException )
                {
                    //service doesn't exist, which is what we want
                }

                RemoveRegistryEntries( serviceName );

                _logger.LogInformation( "Successfully removed service {ServiceName}", serviceName );
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error removing service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Creates the registry parameters for a service
        /// </summary>
        private void CreateParameters( string serviceName, string executablePath, string arguments )
        {
            try
            {
                //get the directory from the executable path 
                //be careful with paths containing spaces
                string appDirectory = Path.GetDirectoryName( executablePath ) ?? string.Empty;

                _logger.LogDebug( "Creating registry parameters: Service={ServiceName}, App={Application}, Args={Arguments}, Dir={Directory}",
                    serviceName, executablePath, arguments, appDirectory );

                var parametersKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                    $"SYSTEM\\CurrentControlSet\\Services\\{serviceName}\\Parameters" );

                if( parametersKey != null )
                {
                    parametersKey.SetValue( "Application", executablePath, RegistryValueKind.ExpandString );
                    parametersKey.SetValue( "AppParameters", arguments, RegistryValueKind.ExpandString );
                    parametersKey.SetValue( "AppDirectory", appDirectory, RegistryValueKind.ExpandString );
                    parametersKey.Close();

                    _logger.LogDebug( "Successfully created service parameters in registry" );
                }
                else
                {
                    _logger.LogWarning( "Failed to create Parameters key in registry" );
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error creating registry parameters for service {ServiceName}", serviceName );
            }
        }

        /// <summary>
        /// Removes registry entries for a service
        /// </summary>
        private void RemoveRegistryEntries( string serviceName )
        {
            try
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(
                    $"SYSTEM\\CurrentControlSet\\Services\\{serviceName}", false );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error removing registry entries for service {ServiceName}", serviceName );
            }
        }

        /// <summary>
        /// Converts a ServiceStartup enum value to the corresponding native value
        /// </summary>
        /// <param name="startupType">The startup type to convert</param>
        /// <returns>The native value for the startup type</returns>
        public uint StartupTypeToNative( ServiceStartup startupType )
        {
            return startupType switch
            {
                ServiceStartup.AutoStart => NativeMethods.SERVICE_AUTO_START,
                ServiceStartup.DelayedAutoStart => NativeMethods.SERVICE_AUTO_START, //delayed is still AUTO_START with additional registry setting
                ServiceStartup.DemandStart => NativeMethods.SERVICE_DEMAND_START,
                ServiceStartup.Disabled => NativeMethods.SERVICE_DISABLED,
                _ => NativeMethods.SERVICE_DEMAND_START
            };
        }

        /// <summary>
        /// Sends a control code to the service
        /// </summary>
        /// <param name="service">The service handle</param>
        /// <param name="code">The control code to send</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool ControlService( IntPtr service, uint code )
        {
            //try to control the service
            ServiceStructures.SERVICE_STATUS status = new ServiceStructures.SERVICE_STATUS();
            if( !_nativeMethods.ControlService( service, code, ref status ) )
            {
                int error = Marshal.GetLastWin32Error();
                _logger.LogWarning( "Failed to control service with code {Code}: {Error}", code, error );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates an existing Windows service
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="displayName">New display name of the service</param>
        /// <param name="description">New description of the service</param>
        /// <param name="executablePath">New path to the executable</param>
        /// <param name="arguments">New command line arguments</param>
        /// <param name="startupType">New service startup type</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool UpdateService(
            string serviceName,
            string displayName,
            string description,
            string executablePath,
            string arguments,
            ServiceStartup startupType = ServiceStartup.AutoStart )
        {
            try
            {
                //reset any previous error
                ResetLastError();

                _logger.LogDebug( "Updating service {ServiceName}", serviceName );

                //convert our startup type to ServiceStartMode
                var startMode = startupType switch
                {
                    ServiceStartup.AutoStart => ServiceStartMode.Automatic,
                    ServiceStartup.DelayedAutoStart => ServiceStartMode.Automatic, //we'll set delayed flag separately
                    ServiceStartup.DemandStart => ServiceStartMode.Manual,
                    ServiceStartup.Disabled => ServiceStartMode.Disabled,
                    _ => ServiceStartMode.Automatic
                };

                var scmHandle = _nativeMethods.OpenSCManager( null, null, NativeMethods.SC_MANAGER_ALL_ACCESS );
                if( scmHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                    _logger.LogError( "Failed to open service control manager: Error {ErrorCode} - {ErrorMessage}",
                        error, errorMessage );
                    SetLastError( error, errorMessage );
                    return false;
                }

                var serviceHandle = _nativeMethods.OpenService(
                    scmHandle,
                    serviceName,
                    NativeMethods.SERVICE_ALL_ACCESS );

                if( serviceHandle == IntPtr.Zero )
                {
                    var error = Marshal.GetLastWin32Error();
                    string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                    _logger.LogError( "Failed to open service {ServiceName}: Error {ErrorCode} - {ErrorMessage}",
                        serviceName, error, errorMessage );
                    _ = _nativeMethods.CloseServiceHandle( scmHandle );
                    SetLastError( error, errorMessage );
                    return false;
                }

                //update display name
                if( !string.IsNullOrEmpty( displayName ) )
                {
                    if( !_nativeMethods.ChangeServiceConfig(
                        serviceHandle,
                        NativeMethods.SERVICE_NO_CHANGE,
                        (uint)startMode,
                        NativeMethods.SERVICE_NO_CHANGE,
                        null,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null,
                        displayName ) )
                    {
                        var error = Marshal.GetLastWin32Error();
                        string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                        _logger.LogError( "Failed to update display name for service {ServiceName}: Error {ErrorCode} - {ErrorMessage}",
                            serviceName, error, errorMessage );
                        _ = _nativeMethods.CloseServiceHandle( serviceHandle );
                        _ = _nativeMethods.CloseServiceHandle( scmHandle );
                        SetLastError( error, errorMessage );
                        return false;
                    }
                }

                //update description
                if( !string.IsNullOrEmpty( description ) )
                {
                    var serviceDescription = new NativeMethods.SERVICE_DESCRIPTION
                    {
                        lpDescription = description
                    };

                    var handle = GCHandle.Alloc( serviceDescription, GCHandleType.Pinned );
                    try
                    {
                        if( !_nativeMethods.ChangeServiceConfig2(
                            serviceHandle,
                            NativeMethods.SERVICE_CONFIG_DESCRIPTION,
                            handle.AddrOfPinnedObject() ) )
                        {
                            var error = Marshal.GetLastWin32Error();
                            string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                            _logger.LogError( "Failed to update description for service {ServiceName}: Error {ErrorCode} - {ErrorMessage}",
                                serviceName, error, errorMessage );
                            _ = _nativeMethods.CloseServiceHandle( serviceHandle );
                            _ = _nativeMethods.CloseServiceHandle( scmHandle );
                            SetLastError( error, errorMessage );
                            return false;
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                //set delayed auto start if needed
                if( startupType == ServiceStartup.DelayedAutoStart )
                {
                    var delayedAutoStart = new NativeMethods.SERVICE_DELAYED_AUTO_START_INFO
                    {
                        fDelayedAutostart = true
                    };

                    var handle = GCHandle.Alloc( delayedAutoStart, GCHandleType.Pinned );
                    try
                    {
                        if( !_nativeMethods.ChangeServiceConfig2(
                            serviceHandle,
                            NativeMethods.SERVICE_CONFIG_DELAYED_AUTO_START_INFO,
                            handle.AddrOfPinnedObject() ) )
                        {
                            var error = Marshal.GetLastWin32Error();
                            string errorMessage = new System.ComponentModel.Win32Exception( error ).Message;
                            _logger.LogWarning( "Failed to set delayed auto start for service {ServiceName}: Error {ErrorCode} - {ErrorMessage}",
                                serviceName, error, errorMessage );
                            //continue despite warning
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                //close handles to ensure smooth exit
                _ = _nativeMethods.CloseServiceHandle( serviceHandle );
                _ = _nativeMethods.CloseServiceHandle( scmHandle );

                //update the registry settings for executable path and arguments
                string servicePath = $"SYSTEM\\CurrentControlSet\\Services\\{serviceName}";
                using( var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( servicePath, true ) )
                {
                    if( key != null )
                    {
                        string nssmExecutablePath = Path.GetDirectoryName( System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ) ?? string.Empty;
                        string nssmPath = Path.Combine( nssmExecutablePath, "NSSM.CLI.exe" );

                        if( !File.Exists( nssmPath ) )
                        {
                            nssmPath = Path.Combine( nssmExecutablePath, "NSSM.exe" );
                        }

                        if( File.Exists( nssmPath ) )
                        {
                            //format the path properly - path should be in quotes if it contains spaces
                            string formattedNssmPath = nssmPath;
                            if( formattedNssmPath.Contains( " " ) )
                            {
                                formattedNssmPath = $"\"{formattedNssmPath}\"";
                            }

                            string quotedServiceName = serviceName;
                            if( quotedServiceName.Contains( " " ) )
                            {
                                quotedServiceName = $"\"{quotedServiceName}\"";
                            }

                            string binaryPathName = $"{formattedNssmPath} --run-as-service {quotedServiceName}";
                            key.SetValue( "ImagePath", binaryPathName );

                            //create or update Parameters key
                            using( var parametersKey = key.CreateSubKey( "Parameters" ) )
                            {
                                if( parametersKey != null )
                                {
                                    if( !string.IsNullOrEmpty( executablePath ) )
                                    {
                                        parametersKey.SetValue( "Application", executablePath );
                                    }

                                    if( arguments != null ) //can be empty string in this case
                                    {
                                        parametersKey.SetValue( "AppParameters", arguments );
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning( "NSSM executable not found at {NssmPath}", nssmPath );
                        }
                    }
                    else
                    {
                        _logger.LogWarning( "Service registry key not found for {ServiceName}", serviceName );
                    }
                }

                _logger.LogInformation( "Service {ServiceName} updated successfully", serviceName );
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error updating service {ServiceName}", serviceName );
                SetLastError( -1, ex.Message );
                return false;
            }
        }
    }
}