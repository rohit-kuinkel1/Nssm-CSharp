using NSSM.Core.Logging;
using NSSM.Core.Services;
using NSSM.Core.Services.Interfaces;
using NSSM.WPF.Models;
using NSSM.WPF.Services;
using System.Windows.Input;

namespace NSSM.WPF.ViewModels;

/// <summary>
/// View model for displaying and editing service details
/// </summary>
public class ServiceDetailsViewModel : ViewModelBase, IParameterViewModel<ServiceModel>
{
    private readonly IWindowsServiceManager _serviceManager;
    private readonly NSSM.Core.Logging.LoggingService _loggingService;
    private readonly IDialogService _dialogService;
    
    private ServiceModel? _service;
    private bool _isEditMode;
    private string _executablePath = string.Empty;
    private string _arguments = string.Empty;
    private string _workingDirectory = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _startupType = string.Empty;

    /// <summary>
    /// Initializes a new instance of the ServiceDetailsViewModel class
    /// </summary>
    /// <param name="serviceManager">Service manager for Windows services</param>
    /// <param name="loggingService">Logging service</param>
    /// <param name="dialogService">Dialog service</param>
    public ServiceDetailsViewModel(
        IWindowsServiceManager serviceManager,
        NSSM.Core.Logging.LoggingService loggingService,
        IDialogService dialogService
    )
    {
        _serviceManager = serviceManager;
        _loggingService = loggingService;
        _dialogService = dialogService;
        
        EditCommand = new RelayCommand(_ => EnterEditMode());
        SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => CancelEdit());
        BrowseExecutableCommand = new RelayCommand(_ => BrowseExecutable());
        BrowseWorkingDirectoryCommand = new RelayCommand(_ => BrowseWorkingDirectory());
    }
    
    /// <summary>
    /// Gets or sets the service model being displayed or edited
    /// </summary>
    public ServiceModel? Service
    {
        get => _service;
        set
        {
            if (SetProperty(ref _service, value) && value != null)
            {
                LoadServiceData(value);
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the service details are in edit mode
    /// </summary>
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }
    
    /// <summary>
    /// Gets or sets the executable path of the service
    /// </summary>
    public string ExecutablePath
    {
        get => _executablePath;
        set => SetProperty(ref _executablePath, value);
    }
    
    /// <summary>
    /// Gets or sets the command-line arguments for the service
    /// </summary>
    public string Arguments
    {
        get => _arguments;
        set => SetProperty(ref _arguments, value);
    }
    
    /// <summary>
    /// Gets or sets the working directory for the service
    /// </summary>
    public string WorkingDirectory
    {
        get => _workingDirectory;
        set => SetProperty(ref _workingDirectory, value);
    }
    
    /// <summary>
    /// Gets or sets the display name of the service
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }
    
    /// <summary>
    /// Gets or sets the description of the service
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    
    /// <summary>
    /// Gets or sets the startup type of the service
    /// </summary>
    public string StartupType
    {
        get => _startupType;
        set => SetProperty(ref _startupType, value);
    }
    
    /// <summary>
    /// Gets a list of available startup types
    /// </summary>
    public List<string> StartupTypes { get; } = new List<string> { "Automatic", "Manual", "Disabled" };
    
    /// <summary>
    /// Gets the command for entering edit mode
    /// </summary>
    public ICommand EditCommand { get; }
    
    /// <summary>
    /// Gets the command for saving changes
    /// </summary>
    public ICommand SaveCommand { get; }
    
    /// <summary>
    /// Gets the command for canceling edits
    /// </summary>
    public ICommand CancelCommand { get; }
    
    /// <summary>
    /// Gets the command for browsing for an executable
    /// </summary>
    public ICommand BrowseExecutableCommand { get; }
    
    /// <summary>
    /// Gets the command for browsing for a working directory
    /// </summary>
    public ICommand BrowseWorkingDirectoryCommand { get; }

    /// <summary>
    /// Initializes the view model with a service model
    /// </summary>
    /// <param name="parameter">Service model to display or edit</param>
    public void Initialize(ServiceModel parameter)
    {
        Service = parameter;
    }
    
    /// <summary>
    /// Loads service data from a service model
    /// </summary>
    /// <param name="service">Service model to load data from</param>
    private void LoadServiceData(ServiceModel service)
    {
        ExecutablePath = service.ExecutablePath;
        Arguments = service.Arguments;
        WorkingDirectory = service.WorkingDirectory;
        DisplayName = service.DisplayName;
        Description = service.Description;
        StartupType = service.StartupType;
        
        IsEditMode = false;
    }
    
    /// <summary>
    /// Enters edit mode for the service details
    /// </summary>
    private void EnterEditMode()
    {
        IsEditMode = true;
    }
    
    /// <summary>
    /// Determines whether the service details can be saved
    /// </summary>
    /// <returns>True if the service details can be saved, false otherwise</returns>
    private bool CanSave()
    {
        return IsEditMode && !string.IsNullOrWhiteSpace(ExecutablePath) && !string.IsNullOrWhiteSpace(DisplayName);
    }
    
    /// <summary>
    /// Saves changes to the service details
    /// </summary>
    private async void Save()
    {
        if (Service == null)
            return;
            
        try
        {
            _loggingService.LogInformation($"Saving changes to service {Service.ServiceName}");
            
            var config = new ServiceUpdateConfig
            {
                ExistingServiceName = Service.ServiceName,
                ServiceName = Service.ServiceName, //we're not allowing name changes for now
                DisplayName = DisplayName,
                Description = Description,
                BinaryPath = ExecutablePath,
                Arguments = Arguments,
                StartType = StartupType
            };
            
            var success = await _serviceManager.UpdateServiceConfigAsync(config);
            
            if (success)
            {
                _loggingService.LogInformation($"Service {Service.ServiceName} updated successfully");
                _dialogService.ShowInfo($"Service '{Service.DisplayName}' was updated successfully.", "Service Updated");
                
                Service.DisplayName = DisplayName;
                Service.Description = Description;
                Service.ExecutablePath = ExecutablePath;
                Service.Arguments = Arguments;
                Service.StartupType = StartupType;
                
                IsEditMode = false;
            }
            else
            {
                var error = await _serviceManager.GetLastErrorAsync();
                string errorMessage = error != null 
                    ? $"Error updating service: {error.ErrorMessage}" 
                    : "Unknown error updating service.";
                    
                _loggingService.LogError(errorMessage);
                _dialogService.ShowError(errorMessage, "Update Failed");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Exception updating service {Service.ServiceName}", ex);
            _dialogService.ShowError($"An error occurred: {ex.Message}", "Update Failed");
        }
    }
    
    /// <summary>
    /// Cancels edits to the service details
    /// </summary>
    private void CancelEdit()
    {
        if (Service != null)
        {
            LoadServiceData(Service);
        }
    }
    
    /// <summary>
    /// Browses for an executable file
    /// </summary>
    private void BrowseExecutable()
    {
        var filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
        var path = _dialogService.ShowOpenFileDialog(filter);
        
        if (!string.IsNullOrEmpty(path))
        {
            ExecutablePath = path;
        }
    }
    
    /// <summary>
    /// Browses for a working directory
    /// </summary>
    private void BrowseWorkingDirectory()
    {
        var path = _dialogService.ShowFolderBrowserDialog();
        
        if (!string.IsNullOrEmpty(path))
        {
            WorkingDirectory = path;
        }
    }
} 