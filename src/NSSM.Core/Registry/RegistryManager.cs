using System.Runtime.Versioning;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NSSM.Core.Service.Enums;

namespace NSSM.Core.Registry
{

    /// <summary>
    /// Handles registry operations for NSSM services
    /// This is a C# implementation of the functionality in registry.cpp from the original tool
    /// </summary>
    [SupportedOSPlatform( "windows" )]
    public class RegistryManager
    {
        private readonly ILogger<RegistryManager> _logger;

        // Registry path constants
        public const string RegistryBasePath = "SYSTEM\\CurrentControlSet\\Services\\{0}";
        public const string RegistryParametersPath = "Parameters";
        public const string RegistryGroupsPath = "SYSTEM\\CurrentControlSet\\Control\\ServiceGroupOrder";
        public const string RegistryGroupsList = "List";

        // Registry value names
        public const string RegExe = "Application";
        public const string RegFlags = "AppParameters";
        public const string RegDir = "AppDirectory";
        public const string RegEnv = "AppEnvironment";
        public const string RegEnvExtra = "AppEnvironmentExtra";
        public const string RegExit = "AppExit";
        public const string RegRestartDelay = "AppRestartDelay";
        public const string RegThrottle = "AppThrottle";
        public const string RegStopMethodSkip = "AppStopMethodSkip";
        public const string RegKillConsoleGracePeriod = "AppStopMethodConsole";
        public const string RegKillWindowGracePeriod = "AppStopMethodWindow";
        public const string RegKillThreadsGracePeriod = "AppStopMethodThreads";
        public const string RegKillProcessTree = "AppKillProcessTree";
        public const string RegStdin = "AppStdin";
        public const string RegStdout = "AppStdout";
        public const string RegStderr = "AppStderr";
        public const string RegStdioSharing = "ShareMode";
        public const string RegStdioDisposition = "CreationDisposition";
        public const string RegStdioFlags = "FlagsAndAttributes";
        public const string RegStdioCopyAndTruncate = "CopyAndTruncate";
        public const string RegHookShareOutputHandles = "AppRedirectHook";
        public const string RegRotate = "AppRotateFiles";
        public const string RegRotateOnline = "AppRotateOnline";
        public const string RegRotateSeconds = "AppRotateSeconds";
        public const string RegRotateBytesLow = "AppRotateBytes";
        public const string RegRotateBytesHigh = "AppRotateBytesHigh";
        public const string RegRotateDelay = "AppRotateDelay";
        public const string RegTimestampLog = "AppTimestampLog";
        public const string RegPriority = "AppPriority";
        public const string RegAffinity = "AppAffinity";
        public const string RegNoConsole = "AppNoConsole";
        public const string RegHook = "AppEvents";

        public RegistryManager( ILogger<RegistryManager> logger )
        {
            _logger = logger;
        }

        /// <summary>
        /// Opens the registry key for a service
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="writable">Whether to open with write access</param>
        /// <returns>Registry key or null if not found</returns>
        [SupportedOSPlatform( "windows" )]
        public RegistryKey? OpenServiceRegistry(
            string serviceName,
            bool writable = false
        )
        {
            try
            {
                string path = string.Format( RegistryBasePath, serviceName );
                return Microsoft.Win32.Registry.LocalMachine.OpenSubKey( path, writable );
            }
            catch( Exception ex )
            {
                if( ex is SecurityException )
                {
                    _logger.LogError( 1003, ex, "Access denied opening registry key for service {ServiceName}", serviceName );
                }
                else if( ex is UnauthorizedAccessException )
                {
                    _logger.LogError( 1004, ex, "Unauthorized access opening registry key for service {ServiceName}", serviceName );
                }
                else
                {
                    _logger.LogError( 1001, ex, "General error opening registry key for service {ServiceName}", serviceName );
                }
                return null;
            }
        }

        [SupportedOSPlatform( "windows" )]
        public RegistryKey? OpenParametersRegistry(
            string serviceName,
            bool writable = false,
            bool create = false
        )
        {
            try
            {
                string path = string.Format( RegistryBasePath, serviceName );

                if( create )
                {
                    var createdServiceKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey( path );
                    return createdServiceKey?.CreateSubKey( RegistryParametersPath );
                }

                var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( path, writable );
                return serviceKey?.OpenSubKey( RegistryParametersPath, writable );
            }
            catch( Exception ex )
            {
                if( ex is SecurityException )
                {
                    _logger.LogError( 1003, ex, "Access denied opening parameters registry key for service {ServiceName}", serviceName );
                }
                else if( ex is UnauthorizedAccessException )
                {
                    _logger.LogError( 1004, ex, "Unauthorized access opening parameters registry key for service {ServiceName}", serviceName );
                }
                else
                {
                    _logger.LogError( 1002, ex, "General error opening parameters registry key for service {ServiceName}", serviceName );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a string value from the registry
        /// </summary>
        /// <param name="key">Registry key</param>
        /// <param name="valueName">Name of the value</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>String value or default if not found</returns>
        public string GetString(
            RegistryKey key,
            string valueName,
            string defaultValue = ""
        )
        {
            try
            {
                var value = key.GetValue( valueName );
                return value?.ToString() ?? defaultValue;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to get string value {ValueName}", valueName );
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a string value in the registry
        /// </summary>
        /// <param name="key">Registry key</param>
        /// <param name="valueName">Name of the value</param>
        /// <param name="value">Value to set</param>
        /// <param name="expandable">Whether to store as an expandable string</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetString(
            RegistryKey key,
            string valueName,
            string value,
            bool expandable = false
        )
        {
            try
            {
                var kind = expandable ? RegistryValueKind.ExpandString : RegistryValueKind.String;
                key.SetValue( valueName, value, kind );
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to set string value {ValueName}", valueName );
                return false;
            }
        }

        /// <summary>
        /// Gets a numeric value from the registry
        /// </summary>
        /// <param name="key">Registry key</param>
        /// <param name="valueName">Name of the value</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Numeric value or default if not found</returns>
        public uint GetNumber(
            RegistryKey key,
            string valueName,
            uint defaultValue = 0
        )
        {
            try
            {
                var value = key.GetValue( valueName );
                if( value == null ) return defaultValue;

                if( value is int intValue )
                    return (uint)intValue;

                if( uint.TryParse( value.ToString(), out uint result ) )
                    return result;

                return defaultValue;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to get numeric value {ValueName}", valueName );
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a numeric value in the registry
        /// </summary>
        /// <param name="key">Registry key</param>
        /// <param name="valueName">Name of the value</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetNumber(
            RegistryKey key,
            string valueName,
            uint value
        )
        {
            try
            {
                key.SetValue( valueName, value, RegistryValueKind.DWord );
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to set numeric value {ValueName}", valueName );
                return false;
            }
        }

        /// <summary>
        /// Creates registry entries for service parameters
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="executablePath">Path to the executable</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool CreateParameters(
            string serviceName,
            string executablePath,
            string arguments,
            string? workingDirectory = null
        )
        {
            try
            {
                using var key = OpenParametersRegistry( serviceName, true, true );
                if( key == null ) return false;

                SetString( key, RegExe, executablePath, true );
                SetString( key, RegFlags, arguments, true );

                //set working directory if provided, otherwise use executable directory
                string directory = workingDirectory ?? Path.GetDirectoryName( executablePath ) ?? string.Empty;
                SetString( key, RegDir, directory, true );

                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to create parameters for service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Removes service parameters from the registry
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <returns>True if successful, false otherwise</returns>
        [SupportedOSPlatform( "windows" )]
        public bool RemoveParameters( string serviceName )
        {
            try
            {
                var serviceKey = OpenServiceRegistry( serviceName, true );
                if( serviceKey == null )
                {
                    _logger.LogWarning( "Could not open service registry key for {ServiceName}", serviceName );
                    return false;
                }

                try
                {
                    serviceKey.DeleteSubKeyTree( RegistryParametersPath, false );
                    _logger.LogInformation( "Removed parameters for service {ServiceName}", serviceName );
                    return true;
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, "Failed to remove parameters for service {ServiceName}: {Message}",
                        serviceName, ex.Message );
                    return false;
                }
                finally
                {
                    serviceKey.Dispose();
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to remove parameters for service {ServiceName}: {Message}",
                    serviceName, ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Gets the exit action for a specific exit code
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="exitCode">Exit code to get action for</param>
        /// <returns>The exit action to take</returns>
        public ExitAction GetExitAction(
            string serviceName,
            uint exitCode
        )
        {
            try
            {
                var key = OpenServiceKey( serviceName );
                if( key == null ) return ExitAction.Restart;

                //check for specific exit code action
                var specificKey = key.OpenSubKey( $"Parameters\\AppExit\\{exitCode}" );
                if( specificKey != null )
                {
                    var specificAction = specificKey.GetValue( "Action" );
                    if( specificAction != null && Enum.TryParse<ExitAction>( specificAction.ToString(), true, out var action ) )
                        return action;
                }

                //check for default action
                var defaultKey = key.OpenSubKey( "Parameters\\AppExit\\Default" );
                if( defaultKey != null )
                {
                    var defaultAction = defaultKey.GetValue( "Action" );
                    if( defaultAction != null && Enum.TryParse<ExitAction>( defaultAction.ToString(), true, out var action ) )
                        return action;
                }

                //default to restart
                return ExitAction.Restart;
            }
            catch
            {
                return ExitAction.Restart;
            }
        }

        /// <summary>
        /// Sets the exit action for a specific exit code
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="exitCode">Exit code to set action for</param>
        /// <param name="action">Action to take</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetExitAction(
            string serviceName,
            string exitCode,
            ExitAction action
        )
        {
            try
            {
                using var parametersKey = OpenParametersRegistry( serviceName, true, true );
                if( parametersKey == null ) return false;

                using var exitKey = parametersKey.CreateSubKey( $"Parameters\\AppExit\\{exitCode}" );
                if( exitKey == null ) return false;

                exitKey.SetValue( "Action", action.ToString(), RegistryValueKind.String );
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to set exit action for service {ServiceName}", serviceName );
                return false;
            }
        }

        /// <summary>
        /// Enumerates all registry values in a key
        /// </summary>
        /// <param name="key">Registry key</param>
        /// <returns>Dictionary of value names and values</returns>
        public Dictionary<string, object> EnumerateValues( RegistryKey key )
        {
            var result = new Dictionary<string, object>();

            try
            {
                foreach( var valueName in key.GetValueNames() )
                {
                    var value = key.GetValue( valueName );
                    if( value != null )
                        result[valueName] = value;
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to enumerate registry values" );
            }

            return result;
        }

        [SupportedOSPlatform( "windows" )]
        public RegistryKey? OpenServiceKey(
            string serviceName,
            bool writable = false
        )
        {
            try
            {
                string path = string.Format( RegistryBasePath, serviceName );
                return Microsoft.Win32.Registry.LocalMachine.OpenSubKey( path, writable );
            }
            catch( Exception ex )
            {
                if( ex is SecurityException )
                {
                    _logger.LogError( 1003, ex, "Access denied opening registry key for service {ServiceName}", serviceName );
                }
                else if( ex is UnauthorizedAccessException )
                {
                    _logger.LogError( 1004, ex, "Unauthorized access opening registry key for service {ServiceName}", serviceName );
                }
                else
                {
                    _logger.LogError( 1001, ex, "General error opening registry key for service {ServiceName}", serviceName );
                }
                return null;
            }
        }

        [SupportedOSPlatform( "windows" )]
        public void CreateServiceKey( string serviceName )
        {
            try
            {
                string path = string.Format( RegistryBasePath, serviceName );
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey( path );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Failed to create registry key for service {ServiceName}", serviceName );
            }
        }
    }
}