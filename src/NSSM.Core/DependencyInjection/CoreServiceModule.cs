using Microsoft.Extensions.DependencyInjection;
using NSSM.Core.Configuration;
using NSSM.Core.Logging;
using NSSM.Core.Messaging;
using NSSM.Core.Repositories;
using NSSM.Core.Repositories.Interfaces;
using NSSM.Core.Service;
using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;
using NSSM.Core.Validation;

namespace NSSM.Core.DependencyInjection;

/// <summary>
/// Centralizes the registration of core services in the dependency injection container.
/// This modular approach improves organization and makes it clear which services
/// are available at the core level.
/// </summary>
public static class CoreServiceModule
{
    /// <summary>
    /// Adds all core services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreServices( this IServiceCollection services )
    {
        services.AddSingleton<AppConfiguration>();
        services.AddSingleton<LoggingService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<ServiceManager>();
        services.AddSingleton<IServiceRegistryManager, ServiceRegistryManager>();
        services.AddSingleton<IServiceRepository, ServiceRepository>();
        services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
        services.AddTransient<ServiceConfigurationValidator>();

        return services;
    }
}
