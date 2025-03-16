using NSSM.Core.Configuration;
using NSSM.Core.Logging;
using NSSM.WPF.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace NSSM.WPF.ViewModels;

/// <summary>
/// View model for application settings
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly LoggingService _loggingService;
    private readonly IDialogService _dialogService;
    private readonly AppConfiguration _configuration;
    
    private bool _isDarkTheme;
    private string _logFileDirectory;
    private int _maxConsoleLines;
    private string _selectedLogLevel;
    
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class
    /// </summary>
    /// <param name="loggingService">Logging service</param>
    /// <param name="dialogService">Dialog service</param>
    /// <param name="configuration">Application configuration</param>
    public SettingsViewModel(
        LoggingService loggingService,
        IDialogService dialogService,
        AppConfiguration configuration
    )
    {
        _loggingService = loggingService;
        _dialogService = dialogService;
        _configuration = configuration;
        
        _isDarkTheme = false; //default to light theme
        _logFileDirectory = _configuration.LogFileDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NSSM",
            "Logs");
        _maxConsoleLines = 1000;
        _selectedLogLevel = "INFO";
        
        LoadSettings();   
        SaveCommand = new RelayCommand(_ => SaveSettings());
        ResetCommand = new RelayCommand(_ => ResetSettings());
        BrowseLogDirectoryCommand = new RelayCommand(_ => BrowseLogDirectory());
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the application uses a dark theme
    /// </summary>
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => SetProperty(ref _isDarkTheme, value);
    }
    
    /// <summary>
    /// Gets or sets the directory where log files are stored
    /// </summary>
    public string LogFileDirectory
    {
        get => _logFileDirectory;
        set => SetProperty(ref _logFileDirectory, value);
    }
    
    /// <summary>
    /// Gets or sets the maximum number of lines to show in the console
    /// </summary>
    public int MaxConsoleLines
    {
        get => _maxConsoleLines;
        set => SetProperty(ref _maxConsoleLines, value);
    }
    
    /// <summary>
    /// Gets or sets the selected log level
    /// </summary>
    public string SelectedLogLevel
    {
        get => _selectedLogLevel;
        set => SetProperty(ref _selectedLogLevel, value);
    }
    
    /// <summary>
    /// Gets available log levels
    /// </summary>
    public ObservableCollection<string> LogLevels { get; } = new ObservableCollection<string>
    {
        "DEBUG",
        "INFO",
        "WARNING",
        "ERROR"
    };
    
    /// <summary>
    /// Gets the command for saving settings
    /// </summary>
    public ICommand SaveCommand { get; }
    
    /// <summary>
    /// Gets the command for resetting settings
    /// </summary>
    public ICommand ResetCommand { get; }
    
    /// <summary>
    /// Gets the command for browsing for a log directory
    /// </summary>
    public ICommand BrowseLogDirectoryCommand { get; }
    
    /// <summary>
    /// Loads settings from app configuration
    /// </summary>
    private void LoadSettings()
    {
        _loggingService.LogDebug("Loading application settings");
        
        try
        {
            IsDarkTheme = _configuration.IsDarkTheme;
            LogFileDirectory = _configuration.LogFileDirectory ?? _logFileDirectory;
            MaxConsoleLines = _configuration.MaxConsoleLines;
            SelectedLogLevel = _configuration.LogLevel ?? "INFO";
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error loading settings", ex);
        }
    }
    
    /// <summary>
    /// Saves settings to app configuration
    /// </summary>
    private void SaveSettings()
    {
        _loggingService.LogDebug("Saving application settings");
        
        try
        {
            _configuration.IsDarkTheme = IsDarkTheme;
            _configuration.LogFileDirectory = LogFileDirectory;
            _configuration.MaxConsoleLines = MaxConsoleLines;
            _configuration.LogLevel = SelectedLogLevel;
            
            _configuration.Save();
            
            _dialogService.ShowInfo("Settings have been saved. Some changes may require a restart to take effect.", "Settings Saved");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error saving settings", ex);
            _dialogService.ShowError($"Could not save settings: {ex.Message}", "Error Saving Settings");
        }
    }
    
    /// <summary>
    /// Resets settings to defaults
    /// </summary>
    private void ResetSettings()
    {
        if (_dialogService.Confirm("Are you sure you want to reset all settings to default values?", "Reset Settings"))
        {
            _loggingService.LogDebug("Resetting application settings to defaults");
            
            IsDarkTheme = false;
            LogFileDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NSSM",
                "Logs");
            MaxConsoleLines = 1000;
            SelectedLogLevel = "INFO";
        }
    }
    
    /// <summary>
    /// Browses for a log directory
    /// </summary>
    private void BrowseLogDirectory()
    {
        var path = _dialogService.ShowFolderBrowserDialog(LogFileDirectory);
        
        if (!string.IsNullOrEmpty(path))
        {
            LogFileDirectory = path;
        }
    }
} 