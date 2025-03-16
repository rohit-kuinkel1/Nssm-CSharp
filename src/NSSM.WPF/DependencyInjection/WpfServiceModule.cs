using Microsoft.Extensions.DependencyInjection;
using NSSM.Core.DependencyInjection;
using NSSM.Core.Services.Interfaces;
using NSSM.WPF.Services;
using NSSM.WPF.ViewModels;

namespace NSSM.WPF.DependencyInjection;

/// <summary>
/// Centralizes the registration of WPF-specific services in the dependency injection container.
/// This modular approach separates UI concerns from core functionality and improves organization.
/// </summary>
public static class WpfServiceModule
{
    /// <summary>
    /// Adds all WPF services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWpfServices( this IServiceCollection services )
    {
        services.AddCoreServices();
        RegisterServices( services );
        RegisterViewModels( services );
        RegisterViews( services );

        return services;
    }

    /// <summary>
    /// Registers application services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    private static void RegisterServices( IServiceCollection services )
    {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
        services.AddSingleton<IDialogService, DialogService>();
    }

    /// <summary>
    /// Registers all view models with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add view models to</param>
    private static void RegisterViewModels( IServiceCollection services )
    {
        //main ViewModel is singleton as it's used throughout the application lifecycle
        services.AddSingleton<MainViewModel>();

        //these ViewModels are transient as they may be created multiple times
        services.AddTransient<InstallServiceViewModel>();
        services.AddTransient<ServiceDetailsViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    /// <summary>
    /// Registers all views with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add views to</param>
    private static void RegisterViews( IServiceCollection services )
    {
        services.AddTransient<MainWindow>();
        services.AddTransient<Views.InstallServiceDialog>();
        services.AddTransient<Views.Pages.ServiceDetailsPage>();
        services.AddTransient<Views.Pages.AboutPage>();
        services.AddTransient<Views.Pages.SettingsPage>();
    }
}
