using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSM.Core.Service;

namespace NSSM.CLI.Commands
{
    /// <summary>
    /// Handler for the 'remove' command which uninstalls an existing Windows service
    /// </summary>
    public class RemoveCommandHandler
    {
        private readonly IServiceProvider _services;

        public RemoveCommandHandler( IServiceProvider services )
        {
            _services = services;
        }

        /// <summary>
        /// Removes an existing Windows service
        /// </summary>
        /// <param name="serviceName">Name of the service to remove</param>
        /// <returns>Success code (0) or error code</returns>
        public async Task<int> HandleAsync( string serviceName )
        {
            var serviceManager = _services.GetRequiredService<ServiceManager>();
            var logger = _services.GetRequiredService<ILogger<RemoveCommandHandler>>();

            Console.WriteLine( $"Removing service '{serviceName}'..." );
            logger.LogInformation( "Removing service {ServiceName}...", serviceName );

            bool result = serviceManager.RemoveService( serviceName );

            if( result )
            {
                Console.WriteLine( $"Service '{serviceName}' removed successfully." );
                logger.LogInformation( "Service {ServiceName} removed successfully.", serviceName );
                return 0;
            }
            else
            {
                Console.WriteLine( $"Failed to remove service '{serviceName}'." );
                logger.LogError( "Failed to remove service {ServiceName}.", serviceName );
                return 1;
            }
        }
    }
}