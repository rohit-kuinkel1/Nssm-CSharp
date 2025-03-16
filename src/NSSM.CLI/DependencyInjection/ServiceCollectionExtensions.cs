using Microsoft.Extensions.DependencyInjection;
using NSSM.CLI.Commands;
using NSSM.CLI.Service;

namespace NSSM.CLI.DependencyInjection
{
    /// <summary>
    /// Extensions methods for configuring services in the CLI application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds CLI-specific services to the service collection
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <returns>The configured service collection</returns>
        public static IServiceCollection AddCliServices( this IServiceCollection services )
        {
            services.AddTransient<InstallCommandHandler>();
            services.AddTransient<RemoveCommandHandler>();
            services.AddTransient<ServiceModeHandler>();

            return services;
        }
    }
}