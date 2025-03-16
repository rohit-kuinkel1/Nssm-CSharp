using NSSM.Core.Constants.AppConstants;
using NSSM.Core.Logging;
using NSSM.Core.Repositories.Interfaces;
using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;

namespace NSSM.Core.Factories
{
    /// <summary>
    /// Implementation of IServiceFactory that creates and validates Windows service configurations.
    /// </summary>
    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the ServiceFactory class.
        /// </summary>
        /// <param name="serviceRepository">Repository for accessing service data</param>
        /// <param name="loggingService">Service for logging</param>
        public ServiceFactory(
            IServiceRepository serviceRepository,
            LoggingService loggingService
        )
        {
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException( nameof( serviceRepository ) );
            _loggingService = loggingService ?? throw new ArgumentNullException( nameof( loggingService ) );
        }

        /// <inheritdoc/>
        public ServiceInstallConfig CreateDefaultInstallConfig()
        {
            _loggingService.LogDebug( $"Creating service instance for default" );

            return new ServiceInstallConfig
            {
                ServiceName = string.Empty,
                DisplayName = string.Empty,
                Description = string.Empty,
                BinaryPath = string.Empty,
                Arguments = string.Empty,
                Account = "LocalSystem",
                StartType = DefaultConstants.DefaultServiceStartType,
                StartImmediately = false
            };
        }

        /// <inheritdoc/>
        public async Task<ServiceUpdateConfig?> CreateUpdateConfigFromExistingAsync( string serviceName )
        {
            if( string.IsNullOrWhiteSpace( serviceName ) )
            {
                _loggingService.LogWarning( "Attempted to create update config with empty service name" );
                return null;
            }

            _loggingService.LogDebug( $"Creating service instance for {serviceName}" );

            try
            {
                var serviceInfo = await _serviceRepository.GetByNameAsync( serviceName );
                if( serviceInfo == null )
                {
                    _loggingService.LogWarning( $"Service not found: {serviceName}" );
                    return null;
                }

                _loggingService.LogDebug( $"Created service instance for {serviceName}" );

                return new ServiceUpdateConfig
                {
                    ExistingServiceName = serviceInfo.Name,
                    ServiceName = serviceInfo.Name,
                    DisplayName = serviceInfo.DisplayName,
                    Description = serviceInfo.Description,
                    BinaryPath = serviceInfo.BinaryPath,
                    Arguments = ExtractArgumentsFromBinaryPath( serviceInfo.BinaryPath ),
                    Account = serviceInfo.Account,
                    StartType = serviceInfo.StartType,
                    StartImmediately = false
                };
            }
            catch( Exception ex )
            {
                _loggingService.LogError( $"Error creating update config for service '{serviceName}': {ex.Message}", ex );
                return null;
            }
        }

        /// <inheritdoc/>
        public (bool IsValid, string[] ValidationErrors) ValidateInstallConfig( ServiceInstallConfig config )
        {
            if( config == null )
            {
                return (false, new[] { "Configuration cannot be null" });
            }

            var errors = new List<string>();

            // Validate required fields
            if( string.IsNullOrWhiteSpace( config.ServiceName ) )
            {
                errors.Add( "Service name is required" );
            }
            else if( config.ServiceName.Length > 256 )
            {
                errors.Add( "Service name exceeds maximum length of 256 characters" );
            }

            if( string.IsNullOrWhiteSpace( config.DisplayName ) )
            {
                errors.Add( "Display name is required" );
            }

            if( string.IsNullOrWhiteSpace( config.BinaryPath ) )
            {
                errors.Add( "Binary path is required" );
            }
            else if( !File.Exists( config.BinaryPath ) )
            {
                errors.Add( $"Binary path does not exist: {config.BinaryPath}" );
            }

            // Validate option values
            if( !IsValidStartType( config.StartType ) )
            {
                errors.Add( $"Invalid start type: {config.StartType}. Valid values are: Automatic, AutomaticDelayed, Manual, Disabled" );
            }

            if( !IsValidAccount( config.Account ) )
            {
                errors.Add( $"Invalid account: {config.Account}. Valid values are: LocalSystem, LocalService, NetworkService, or a specific user account" );
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <inheritdoc/>
        public (bool IsValid, string[] ValidationErrors) ValidateUpdateConfig( ServiceUpdateConfig config )
        {
            if( config == null )
            {
                return (false, new[] { "Configuration cannot be null" });
            }

            var errors = new List<string>();

            // Add update-specific validations
            if( string.IsNullOrWhiteSpace( config.ExistingServiceName ) )
            {
                errors.Add( "Existing service name is required" );
            }

            // Add install validations
            var (isValid, validationErrors) = ValidateInstallConfig( config );
            if( !isValid )
            {
                errors.AddRange( validationErrors );
            }

            return (errors.Count == 0, errors.ToArray());
        }

        #region Private Helper Methods

        private string ExtractArgumentsFromBinaryPath( string binaryPath )
        {
            //might need to be enhanced to handle various quoting patterns and escape characters
            var parts = binaryPath.Split( new[] { ' ' }, 2 );
            return parts.Length > 1 ? parts[1] : string.Empty;
        }

        private bool IsValidStartType( string startType )
        {
            var validStartTypes = new[] { "Automatic", "AutomaticDelayed", "Manual", "Disabled" };
            return validStartTypes.Contains( startType, StringComparer.OrdinalIgnoreCase );
        }

        private bool IsValidAccount( string account )
        {
            var systemAccounts = new[] { "LocalSystem", "LocalService", "NetworkService" };

            //sys accounts are always valid
            if( systemAccounts.Contains( account, StringComparer.OrdinalIgnoreCase ) )
            {
                return true;
            }

            //for user accounts, we just check if it's not empty
            //more sophisticated implementation would validate against AD/local accounts but local usr check is enough for us for now
            return !string.IsNullOrWhiteSpace( account );
        }

        #endregion
    }
}