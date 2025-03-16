using Microsoft.Extensions.DependencyInjection;
using NSSM.Core.Logging;
using NSSM.WPF.Services;
using NSSM.WPF.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;

namespace NSSM.WPF.Views
{
    /// <summary>
    /// Interaction logic for InstallServiceDialog.xaml
    /// </summary>
    public partial class InstallServiceDialog : Window
    {
        private InstallServiceViewModel? _viewModel;

        public InstallServiceDialog()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(Window owner)
        {
            LoggingService? loggingService = null;
            try
            {
                // Get LoggingService from DI container
                loggingService = App.ServiceProvider.GetService<LoggingService>();
                loggingService?.LogDebug("Starting InstallServiceDialog.ShowDialog");
            }
            catch
            {
                Debug.WriteLine("Could not get LoggingService in InstallServiceDialog");
            }

            try
            {
                // Create dialog
                loggingService?.LogDebug("Creating InstallServiceDialog instance");
                var dialog = new InstallServiceDialog
                {
                    Owner = owner
                };

                // Get the view model from DI container
                loggingService?.LogDebug("Getting InstallServiceViewModel from service provider");
                var viewModel = App.ServiceProvider.GetRequiredService<InstallServiceViewModel>();
                
                loggingService?.LogDebug("Setting ViewModel to dialog DataContext");
                dialog._viewModel = viewModel;
                dialog.DataContext = viewModel;

                // Subscribe to the completed event
                loggingService?.LogDebug("Subscribing to InstallationCompleted event");
                viewModel.InstallationCompleted += (sender, args) =>
                {
                    loggingService?.LogDebug($"InstallationCompleted event fired with result: {args.Result.Success}");
                    if (args.Result.Success)
                    {
                        dialog.DialogResult = true;
                    }
                };

                // Show dialog
                loggingService?.LogDebug("Calling dialog.ShowDialog()");
                bool? dialogResult = dialog.ShowDialog();
                loggingService?.LogDebug($"dialog.ShowDialog() returned: {dialogResult}");
                
                return dialogResult;
            }
            catch (Exception ex)
            {
                loggingService?.LogError($"Error showing install dialog: {ex.Message}", ex);
                return null;
            }
        }

        public static bool ShowDialog(Window owner, out ServiceInstallResult result)
        {
            LoggingService? loggingService = null;
            result = new ServiceInstallResult();
            
            try
            {
                // Get LoggingService from DI container
                loggingService = App.ServiceProvider.GetService<LoggingService>();
                loggingService?.LogDebug("Starting InstallServiceDialog.ShowDialog with result parameter");
            }
            catch
            {
                Debug.WriteLine("Could not get LoggingService in InstallServiceDialog");
            }

            try
            {
                // Create dialog
                loggingService?.LogDebug("Creating InstallServiceDialog instance");
                var dialog = new InstallServiceDialog
                {
                    Owner = owner
                };

                // Get the view model from DI container
                loggingService?.LogDebug("Getting InstallServiceViewModel from service provider");
                var viewModel = App.ServiceProvider.GetRequiredService<InstallServiceViewModel>();
                
                loggingService?.LogDebug("Setting ViewModel to dialog DataContext");
                dialog._viewModel = viewModel;
                dialog.DataContext = viewModel;

                // Create result object
                ServiceInstallResult installResult = new ServiceInstallResult();
                
                // Subscribe to the completed event
                loggingService?.LogDebug("Subscribing to InstallationCompleted event");
                viewModel.InstallationCompleted += (sender, args) =>
                {
                    loggingService?.LogDebug($"InstallationCompleted event fired with result: {args.Result.Success}");
                    installResult = args.Result;
                    if (args.Result.Success)
                    {
                        dialog.DialogResult = true;
                    }
                };

                // Show dialog
                loggingService?.LogDebug("Calling dialog.ShowDialog()");
                bool? dialogResult = dialog.ShowDialog();
                loggingService?.LogDebug($"dialog.ShowDialog() returned: {dialogResult}");
                
                // Set the result
                result = installResult;
                
                // Return true if dialog result is true
                return dialogResult == true;
            }
            catch (Exception ex)
            {
                loggingService?.LogError($"Error showing install dialog: {ex.Message}", ex);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return false;
            }
        }
    }

    public class ServiceInstallResult
    {
        public bool Success { get; set; }
        public string? ServiceName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class InstallationCompleted : EventArgs
    {
        public ServiceInstallResult Result { get; }

        public InstallationCompleted(ServiceInstallResult result)
        {
            Result = result;
        }
    }
}
