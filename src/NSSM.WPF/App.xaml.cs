using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSM.Core.Constants;
using NSSM.Core.Constants.AppConstants;
using NSSM.Core.Logging;
using NSSM.Core.Messaging;
using NSSM.WPF.DependencyInjection;
using NSSM.WPF.Services;
using NSSM.WPF.ViewModels;
using NSSM.WPF.Views;
using NSSM.WPF.Views.Pages;
using System;
using System.IO;
using System.Windows;

namespace NSSM.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private static ServiceProvider? _serviceProvider;
    private LoggingService? _loggingService;
    
    public static ServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Service provider is not initialized");

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Create a log file for debugging
            string logPath = Path.Combine(
                Environment.ExpandEnvironmentVariables(FileConstants.LogFileDirectory), 
                FileConstants.DebugLogFileName);
                
            using (StreamWriter sw = File.CreateText(logPath))
            {
                sw.WriteLine($"Application starting at: {DateTime.Now}");
                sw.WriteLine("Configuring services...");
            }

            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            // Initialize logging service after DI container is built
            _loggingService = _serviceProvider.GetRequiredService<LoggingService>();
            _loggingService.LogInformation("Services configured, creating main window...");

            // Configure navigation routes
            ConfigureNavigation();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            
            mainWindow.DataContext = mainViewModel;
            
            _loggingService.LogDebug("Setting main window as application's main window");
            MainWindow = mainWindow;
            
            mainWindow.Show();
            _loggingService.LogDebug("Window shown, startup complete");
            
            // Subscribe to unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }
        catch (Exception ex)
        {
            string errorLogPath = Path.Combine(
                Environment.ExpandEnvironmentVariables(FileConstants.LogFileDirectory), 
                FileConstants.ErrorLogFileName);
                
            using (StreamWriter sw = File.CreateText(errorLogPath))
            {
                sw.WriteLine($"Error during startup: {ex.Message}");
                sw.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    sw.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    sw.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
            }
            
            // Show a message box with the error and file path
            System.Windows.MessageBox.Show(
                $"Application failed to start: {ex.Message}\nCheck log file at: {errorLogPath}", 
                UIConstants.ErrorDialogTitle, 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
                
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register logging services
        services.AddLogging(configure => 
        {
            configure.AddConsole();
            configure.AddDebug();
            // Set minimum log level
            configure.SetMinimumLevel(LogLevel.Debug);
        });
        
        // Add all services using the modular architecture
        services.AddWpfServices();
    }
    
    /// <summary>
    /// Configures navigation routes for the application
    /// </summary>
    private void ConfigureNavigation()
    {
        var navigationService = _serviceProvider!.GetRequiredService<INavigationService>();
        
        // Register Windows
        navigationService.RegisterWindow<InstallServiceViewModel, InstallServiceDialog>();
        
        // Register Pages
        navigationService.RegisterPage<ServiceDetailsViewModel, ServiceDetailsPage>();
        navigationService.RegisterPage<SettingsViewModel, SettingsPage>();
        navigationService.RegisterPage<AboutViewModel, AboutPage>();
        
        _loggingService?.LogDebug("Navigation routes configured");
    }
    
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledException(e.ExceptionObject as Exception);
    }

    private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogUnhandledException(e.Exception);
        e.Handled = true;
    }
    
    private void LogUnhandledException(Exception? ex)
    {
        if (ex == null) return;
        
        try
        {
            _loggingService?.LogError($"Unhandled exception: {ex.Message}", ex);
            
            System.Windows.MessageBox.Show(
                $"An unexpected error occurred: {ex.Message}\nThe error has been logged.",
                UIConstants.ErrorDialogTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            // Fallback if logging fails
            string errorLogPath = Path.Combine(
                Environment.ExpandEnvironmentVariables(FileConstants.LogFileDirectory), 
                FileConstants.ErrorLogFileName);
                
            try
            {
                using StreamWriter sw = File.AppendText(errorLogPath);
                sw.WriteLine($"{DateTime.Now}: Unhandled exception: {ex.Message}");
                sw.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            catch
            {
                // Last resort - we can't log the error
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _loggingService?.LogInformation("Application shutting down");
            
            // Force garbage collection to release resources
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            // Clean up the service provider
            _serviceProvider?.Dispose();
            
            // Unsubscribe from events
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
            
            // Ensure all background threads are terminate
            _loggingService?.LogInformation("Application shutdown complete");
            
            // Exit with force if needed - only use this as a last resort
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        catch (Exception ex)
        {
            try
            {
                _loggingService?.LogError($"Error during application shutdown: {ex.Message}", ex);
            }
            finally
            {
                // As a last resort, force the process to terminate
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
        finally
        {
            base.OnExit(e);
        }
    }
}
