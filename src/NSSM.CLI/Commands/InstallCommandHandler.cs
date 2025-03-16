using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSM.Core.Service;
using NSSM.Core.Service.Enums;

namespace NSSM.CLI.Commands
{
    /// <summary>
    /// Handler for the 'install' command which creates a new Windows service
    /// </summary>
    public class InstallCommandHandler
    {
        private readonly IServiceProvider _services;

        public InstallCommandHandler( IServiceProvider services )
        {
            _services = services;
        }

        /// <summary>
        /// Creates and installs a Windows service with the specified parameters
        /// </summary>
        /// <param name="serviceName">Unique name of the service</param>
        /// <param name="executablePath">Path to the executable file to run</param>
        /// <param name="displayName">User-friendly name to display in service manager</param>
        /// <param name="description">Detailed description of the service</param>
        /// <param name="arguments">Command-line arguments to pass to the executable</param>
        /// <param name="startup">Service startup type (auto, delayed, manual, disabled)</param>
        /// <param name="username">Optional username to run service under (null for LocalSystem)</param>
        /// <param name="password">Optional password for the username</param>
        /// <returns>Success code (0) or error code</returns>
        public async Task<int> HandleAsync(
            string serviceName,
            string executablePath,
            string? displayName,
            string? description,
            string? arguments,
            ServiceStartup startup,
            string? username,
            string? password
        )
        {
            var serviceManager = _services.GetRequiredService<ServiceManager>();
            var logger = _services.GetRequiredService<ILogger<InstallCommandHandler>>();

            displayName ??= serviceName;
            description ??= string.Empty;
            arguments ??= string.Empty;

            Console.WriteLine( $"Installing service '{serviceName}'..." );
            logger.LogInformation( "Installing service {ServiceName}...", serviceName );

            bool result = serviceManager.InstallService(
                serviceName,
                displayName,
                description,
                executablePath,
                arguments,
                startup,
                username,
                password
            );

            if( result )
            {
                Console.WriteLine( $"Service '{serviceName}' installed successfully." );
                logger.LogInformation( "Service {ServiceName} installed successfully.", serviceName );
                return 0;
            }
            else
            {
                Console.WriteLine( $"Failed to install service '{serviceName}'." );
                logger.LogError( "Failed to install service {ServiceName}.", serviceName );
                return 1;
            }
        }
    }
}