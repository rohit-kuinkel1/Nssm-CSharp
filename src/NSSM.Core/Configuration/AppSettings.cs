using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NSSM.Core.Configuration;

/// <summary>
/// Provides centralized access to application configuration settings.
/// This implementation uses Microsoft's configuration system to load settings from various sources.
/// </summary>
public static class AppSettings
{
    //for testing purposes
    internal static Func<IConfiguration>? ConfigurationFactory { get; set; }

    /// <summary>
    /// Adds application configuration to the dependency injection container.
    /// This method configures settings from appsettings.json, environment variables, and command line arguments.
    /// </summary>
    /// <param name="services">The service collection to add configuration to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAppConfiguration( this IServiceCollection services )
    {
        if( ConfigurationFactory != null )
        {
            services.AddSingleton( ConfigurationFactory() );
            return services;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath( Directory.GetCurrentDirectory() )
            .AddJsonFile( "appsettings.json", optional: false )
            .Build();

        services.AddSingleton<IConfiguration>( configuration );

        services.Configure<ServiceManagerOptions>( options =>
            configuration.GetSection( "ServiceManager" ).Bind( options ) );
        services.Configure<LoggingOptions>( options =>
            configuration.GetSection( "Logging" ).Bind( options ) );
        services.Configure<UIOptions>( options =>
            configuration.GetSection( "UI" ).Bind( options ) );

        services.AddLogging();

        return services;
    }

    /// <summary>
    /// Gets the current environment name, defaulting to "Production" if not specified.
    /// </summary>
    /// <returns>The environment name</returns>
    private static string GetEnvironment()
    {
        return System.Environment.GetEnvironmentVariable( "NSSM_ENVIRONMENT" ) ?? "Production";
    }
}

