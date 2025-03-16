using Microsoft.Win32;
using NSSM.Core.Logging;
using NSSM.Core.Messaging;
using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;
using NSSM.WPF.Views;
using System.IO;
using System.Windows.Input;
using System.Windows;

namespace NSSM.WPF.ViewModels;

/// <summary>
/// ViewModel for installing a new Windows service.
/// </summary>
public class InstallServiceViewModel : ViewModelBase
{
    private readonly IWindowsServiceManager _serviceManager;
    private readonly LoggingService _loggingService;
    private readonly IMessageBus _messageBus;
    
    private string _serviceName = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _executablePath = string.Empty;
    private string _arguments = string.Empty;
    private string _workingDirectory = string.Empty;

    public event EventHandler<InstallationCompleted>? InstallationCompleted;

    /// <summary>
    /// Initializes a new instance of the InstallServiceViewModel class.
    /// </summary>
    /// <param name="serviceManager">Windows service manager interface</param>
    /// <param name="loggingService">Logging service</param>
    /// <param name="messageBus">Message bus for event communication</param>
    public InstallServiceViewModel(
        IWindowsServiceManager serviceManager,
        LoggingService loggingService,
        IMessageBus messageBus)
    {       
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        
        // Initialize commands
        BrowseExecutableCommand = new RelayCommand(_ => BrowseExecutable());
        BrowseWorkingDirCommand = new RelayCommand(_ => BrowseWorkingDirectory());
        InstallCommand = new RelayCommand(_ => InstallService(), _ => CanInstallService());
    }

    public string ServiceName
    {
        get => _serviceName;
        set
        {
            if (SetProperty(ref _serviceName, value))
            {
                // If display name is empty or matches the previous service name, update it
                if (string.IsNullOrEmpty(_displayName) || _displayName == _serviceName)
                {
                    DisplayName = value;
                }
                
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string ExecutablePath
    {
        get => _executablePath;
        set
        {
            if (SetProperty(ref _executablePath, value))
            {
                // If working directory is empty, set it to the executable's directory
                if (string.IsNullOrEmpty(_workingDirectory) && !string.IsNullOrEmpty(value))
                {
                    try
                    {
                        WorkingDirectory = Path.GetDirectoryName(value) ?? string.Empty;
                    }
                    catch
                    {
                        // Ignore any exceptions
                    }
                }
                
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Arguments
    {
        get => _arguments;
        set => SetProperty(ref _arguments, value);
    }

    public string WorkingDirectory
    {
        get => _workingDirectory;
        set => SetProperty(ref _workingDirectory, value);
    }

    public ICommand BrowseExecutableCommand { get; }
    public ICommand BrowseWorkingDirCommand { get; }
    public ICommand InstallCommand { get; }

    private void BrowseExecutable()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Title = "Select Executable"
        };
        
        if (dialog.ShowDialog() == true)
        {
            ExecutablePath = dialog.FileName;
        }
    }

    private void BrowseWorkingDirectory()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Working Directory",
            CheckFileExists = false,
            FileName = "Folder Selection"
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                WorkingDirectory = selectedPath;
            }
        }
    }

    private bool CanInstallService()
    {
        return !string.IsNullOrWhiteSpace(ServiceName) && !string.IsNullOrWhiteSpace(ExecutablePath);
    }

    private async void InstallService()
    {
        if (_serviceManager == null)
        {
            _loggingService.LogError("Service manager not available for service installation");
            OnInstallationCompleted(new ServiceInstallResult
            {
                Success = false,
                ErrorMessage = "Service manager not available"
            });
            return;
        }

        try
        {
            _loggingService.LogDebug($"Attempting to install service '{ServiceName}'");
            
            // Check if the executable looks like a regular application not suitable for services
            bool isRegularApplication = IsLikelyEndUserApplication(ExecutablePath);
            if (isRegularApplication)
            {
                _loggingService.LogWarning($"The selected file '{Path.GetFileName(ExecutablePath)}' appears to be a regular application which might not work as a service");
                
                // We'll still attempt to install, but warn the user that it may not work
                var result = System.Windows.MessageBox.Show(
                    $"The selected file '{Path.GetFileName(ExecutablePath)}' appears to be a regular desktop application.\n\n" +
                    "Most desktop applications (such as Steam, browsers, etc.) are not designed to run as Windows services and will likely fail.\n\n" +
                    "Windows services require applications specifically built to function as services. " +
                    "NSSM can run regular applications as services by acting as a wrapper, but functionality may be limited.\n\n" +
                    "Do you want to continue anyway?",
                    "Warning: Not a Service Application",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    _loggingService.LogInformation("User cancelled installation after application compatibility warning");
                    return;
                }
                
                _loggingService.LogInformation("User chose to continue with installation despite application compatibility warning");
            }
            
            var config = new ServiceInstallConfig
            {
                ServiceName = ServiceName,
                DisplayName = DisplayName,
                BinaryPath = ExecutablePath,
                Arguments = Arguments,
                Description = Description,
                Account = "LocalSystem",  // Default to LocalSystem
                StartType = "Automatic",  // Default to Automatic
                StartImmediately = false  // Don't start immediately
            };

            _loggingService.LogDebug($"Service config: Name='{config.ServiceName}', Path='{config.BinaryPath}', Args='{config.Arguments}'");

            var success = await _serviceManager.InstallServiceAsync(config);
            
            if (success)
            {
                _loggingService.LogDebug($"Service installation API call returned success for '{ServiceName}'");
            }
            else
            {
                _loggingService.LogWarning($"Service installation API call returned failure for '{ServiceName}'");
                
                // Retrieve the last error details from the service manager
                var lastError = await _serviceManager.GetLastErrorAsync();
                
                // Provide a more specific error message based on the executable type and error code
                string errorMessage = "Failed to install service";
                
                if (lastError != null && lastError.ErrorCode != 0)
                {
                    // Log the specific Win32 error details
                    _loggingService.LogError($"Service installation failed with Win32 error code {lastError.ErrorCode}: {lastError.ErrorMessage}");
                    
                    // Format a user-friendly error message based on the error code
                    errorMessage = FormatServiceInstallationError(lastError.ErrorCode, lastError.ErrorMessage, isRegularApplication);
                }
                else if (isRegularApplication)
                {
                    errorMessage = $"Failed to install service. The application '{Path.GetFileName(ExecutablePath)}' might not be designed to run as a service. Try using NSSM's built-in wrapper mode.";
                }
                
                OnInstallationCompleted(new ServiceInstallResult
                {
                    Success = false,
                    ServiceName = ServiceName,
                    ErrorMessage = errorMessage
                });
                return;
            }

            OnInstallationCompleted(new ServiceInstallResult
            {
                Success = success,
                ServiceName = ServiceName,
                ErrorMessage = success ? null : "Failed to install service"
            });

            // Notify other components about the service installation
            if (success)
            {
                var serviceInfo = new ServiceInfo
                {
                    Name = ServiceName,
                    DisplayName = DisplayName,
                    Description = Description,
                    BinaryPath = ExecutablePath,
                    Account = "LocalSystem",
                    StartType = "Automatic",
                    Status = "Stopped"  // Initial status
                };
                
                _loggingService.LogInformation($"Service '{ServiceName}' installed successfully, publishing message");
                _messageBus.Publish(new ServiceInstalledMessage(serviceInfo));
                _loggingService.LogInformation($"Service '{ServiceName}' installed successfully");
                
                // Verify the service is in the list of services
                try
                {
                    _loggingService.LogDebug($"Verifying service '{ServiceName}' exists in service manager");
                    var verifyService = await _serviceManager.GetServiceInfoAsync(ServiceName);
                    
                    if (verifyService != null)
                    {
                        _loggingService.LogDebug($"Verification successful, service '{ServiceName}' found");
                    }
                    else
                    {
                        _loggingService.LogWarning($"Verification failed: service '{ServiceName}' not found after installation");
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"Error verifying service '{ServiceName}' installation: {ex.Message}", ex);
                }
            }
            else
            {
                _loggingService.LogWarning($"Failed to install service '{ServiceName}'");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error installing service '{ServiceName}': {ex.Message}", ex);
            OnInstallationCompleted(new ServiceInstallResult
            {
                Success = false,
                ServiceName = ServiceName,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Checks if the executable is likely a regular end-user application not designed to run as a service
    /// </summary>
    private bool IsLikelyEndUserApplication(string exePath)
    {
        try
        {
            if (string.IsNullOrEmpty(exePath))
                return false;
                
            string fileName = Path.GetFileName(exePath).ToLowerInvariant();
            string directory = Path.GetDirectoryName(exePath) ?? string.Empty;
            
            // Check for common applications that are not suitable as services
            string[] knownUserApps = new[] { 
                "steam.exe", "chrome.exe", "firefox.exe", "msedge.exe", "brave.exe", 
                "outlook.exe", "winword.exe", "excel.exe", "powerpnt.exe",
                "explorer.exe", "notepad.exe", "calc.exe", "mspaint.exe",
                "vlc.exe", "wmplayer.exe", "devenv.exe", "code.exe", "spotify.exe"
            };
            
            if (knownUserApps.Contains(fileName))
                return true;
                
            // Check if it's in Program Files, which often contains end-user applications
            if (directory.Contains("program files", StringComparison.OrdinalIgnoreCase) && 
                !directory.Contains("common files", StringComparison.OrdinalIgnoreCase))
                return true;
                
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Formats a user-friendly error message for service installation failures
    /// </summary>
    private string FormatServiceInstallationError(int errorCode, string errorMessage, bool isRegularApplication)
    {
        // Start with the raw error information
        string formattedMessage = $"Service installation failed: Error {errorCode} - {errorMessage}";
        
        // Add additional context based on known error codes
        switch (errorCode)
        {
            case 5: // ERROR_ACCESS_DENIED
                formattedMessage = $"Access denied while installing service. Make sure you're running as Administrator. Error: {errorMessage}";
                break;
                
            case 1073: // ERROR_SERVICE_EXISTS
                formattedMessage = $"A service with this name already exists. Error: {errorMessage}";
                break;
                
            case 1072: // ERROR_SERVICE_MARKED_FOR_DELETE
                formattedMessage = $"The service is marked for deletion. Please wait a moment and try again. Error: {errorMessage}";
                break;
                
            case 123: // ERROR_INVALID_NAME
                formattedMessage = $"The service name contains invalid characters. Error: {errorMessage}";
                break;
                
            case 87: // ERROR_INVALID_PARAMETER
                formattedMessage = $"Invalid parameter while creating service. Error: {errorMessage}";
                break;
                
            case 1057: // ERROR_INVALID_SERVICE_ACCOUNT
                formattedMessage = $"The specified service account is invalid. Windows could not authenticate with the account credentials.\n\n" +
                                    "For LocalSystem account, make sure you're using 'LocalSystem' as the account name without a password.\n\n" +
                                    "For a custom account, make sure the account exists and the password is correct.\n\n" +
                                    "Error: {errorMessage}";
                break;
                
            case 1059: // ERROR_CIRCULAR_DEPENDENCY
                formattedMessage = $"A circular service dependency was specified. Error: {errorMessage}";
                break;
                
            case 1078: // ERROR_DUPLICATE_SERVICE_NAME
                formattedMessage = $"The display name is already used by another service. Error: {errorMessage}";
                break;
                
            case 1060: // ERROR_SERVICE_DOES_NOT_EXIST
                formattedMessage = $"The service does not exist in the SCM database. Error: {errorMessage}";
                break;
                
            case 1053: // ERROR_SERVICE_REQUEST_TIMEOUT
                formattedMessage = $"Service request timed out. Error: {errorMessage}";
                break;
                
            case 1069: // ERROR_SERVICE_SPECIFIC_ERROR
                formattedMessage = $"The service reported a service-specific error. Error: {errorMessage}";
                break;
                
            case 1921: // ERROR_SERVICE_CANNOT_ACCEPT_CTRL
                formattedMessage = $"The service cannot accept control messages at this time. Error: {errorMessage}";
                break;
                
            case 1055: // ERROR_SERVICE_DATABASE_LOCKED
                formattedMessage = $"The service database is locked. Error: {errorMessage}";
                break;
                
            default:
                // For unknown error codes, present the raw information
                formattedMessage = $"Service installation failed: Error {errorCode} - {errorMessage}.\n\nPlease check Windows Event Viewer for more details.";
                break;
        }
        
        // Add additional context for regular applications
        if (isRegularApplication)
        {
            formattedMessage += "\n\nNote: The application you selected appears to be a regular desktop application which may not function correctly as a service.";
        }
        
        return formattedMessage;
    }

    private void OnInstallationCompleted(ServiceInstallResult result)
    {
        InstallationCompleted?.Invoke(this, new InstallationCompleted(result));
    }
}
