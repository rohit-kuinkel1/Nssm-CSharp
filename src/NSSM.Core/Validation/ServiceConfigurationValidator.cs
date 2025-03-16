using NSSM.Core.Constants;
using NSSM.Core.Models;
namespace NSSM.Core.Validation
{
    /// <summary>
    /// Validates ServiceConfiguration objects to ensure they meet the requirements for service operations.
    /// This class helps catch configuration errors early before attempting service operations.
    /// </summary>
    public class ServiceConfigurationValidator
    {
        /// <summary>
        /// Validates a service configuration for completeness and correctness
        /// </summary>
        /// <param name="config">The service configuration to validate</param>
        /// <param name="errors">Output list of validation errors if any are found</param>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        public static bool Validate(
            ServiceConfiguration config,
            out IList<string> errors
        )
        {
            errors = new List<string>();

            if( string.IsNullOrWhiteSpace( config.ServiceName ) )
            {
                errors.Add( "Service name is required" );
            }
            else if( config.ServiceName.Length > ServiceLengthConstants.ServiceName )
            {
                errors.Add( $"Service name exceeds the maximum length of {ServiceLengthConstants.ServiceName} characters" );
            }

            //check for invalid characters in service name
            if( config.ServiceName.Contains( "/" ) || config.ServiceName.Contains( "\\" ) ||
                config.ServiceName.Contains( ":" ) || config.ServiceName.Contains( "*" ) ||
                config.ServiceName.Contains( "?" ) || config.ServiceName.Contains( "\"" ) ||
                config.ServiceName.Contains( "<" ) || config.ServiceName.Contains( ">" ) ||
                config.ServiceName.Contains( "|" ) )
            {
                errors.Add( "Service name contains invalid characters" );
            }

            if( string.IsNullOrWhiteSpace( config.ExecutablePath ) )
            {
                errors.Add( "Executable path is required" );
            }
            else if( config.ExecutablePath.Length > ServiceLengthConstants.ExecutablePath )
            {
                errors.Add( $"Executable path exceeds the maximum length of {ServiceLengthConstants.ExecutablePath} characters" );
            }
            else if( !File.Exists( config.ExecutablePath ) )
            {
                errors.Add( $"Executable file does not exist: {config.ExecutablePath}" );
            }

            if( config.Arguments != null && config.Arguments.Length > ServiceLengthConstants.CommandLine )
            {
                errors.Add( $"Command line arguments exceed the maximum length of {ServiceLengthConstants.CommandLine} characters" );
            }

            if( !string.IsNullOrEmpty( config.WorkingDirectory ) )
            {
                if( config.WorkingDirectory.Length > ServiceLengthConstants.Directory )
                {
                    errors.Add( $"Working directory path exceeds the maximum length of {ServiceLengthConstants.Directory} characters" );
                }
                else if( !Directory.Exists( config.WorkingDirectory ) )
                {
                    errors.Add( $"Working directory does not exist: {config.WorkingDirectory}" );
                }
            }

            if( !string.IsNullOrEmpty( config.LogDirectory ) )
            {
                try
                {
                    Directory.CreateDirectory( config.LogDirectory );
                }
                catch( Exception ex )
                {
                    errors.Add( $"Cannot create log directory: {config.LogDirectory}. Error: {ex.Message}" );
                }
            }

            if( config.StopTimeoutMs < 0 )
            {
                errors.Add( "Stop timeout cannot be negative" );
            }

            if( config.StartupDelayMs < 0 )
            {
                errors.Add( "Startup delay cannot be negative" );
            }

            if( config.RestartThrottleMs < 0 )
            {
                errors.Add( "Restart throttle cannot be negative" );
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Validates a service configuration and throws an exception if it's invalid
        /// </summary>
        /// <param name="config">The service configuration to validate</param>
        /// <exception cref="ArgumentException">Thrown if the configuration is invalid</exception>
        public static void ValidateAndThrow( ServiceConfiguration config )
        {
            if( !Validate( config, out var errors ) )
            {
                throw new ArgumentException( $"Invalid service configuration: {string.Join( ", ", errors )}" );
            }
        }
    }
}