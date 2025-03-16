using System.Text.Json;
using NSSM.Core.Logging;

namespace NSSM.Core.Configuration;

/// <summary>
/// Class for storing and retrieving application configuration settings.
/// </summary>
public class AppConfiguration
{
    private readonly string _configPath;
    private readonly LoggingService? _loggingService;

    /// <summary>
    /// Gets or sets a value indicating whether the application uses the dark theme.
    /// </summary>
    public bool IsDarkTheme { get; set; }

    /// <summary>
    /// Gets or sets the directory where log files are stored.
    /// </summary>
    public string? LogFileDirectory { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of lines to show in the console.
    /// </summary>
    public int MaxConsoleLines { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the log level (DEBUG, INFO, WARNING, ERROR).
    /// </summary>
    public string? LogLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the AppConfiguration class.
    /// </summary>
    /// <param name="loggingService">Logging service for diagnostics.</param>
    public AppConfiguration( LoggingService? loggingService = null )
    {
        _loggingService = loggingService;

        var appDataPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
        var nssmFolder = Path.Combine( appDataPath, "NSSM" );

        if( !Directory.Exists( nssmFolder ) )
        {
            Directory.CreateDirectory( nssmFolder );
        }

        _configPath = Path.Combine( nssmFolder, "config.json" );

        // Set defaults
        IsDarkTheme = false;
        LogFileDirectory = Path.Combine( nssmFolder, "Logs" );
        LogLevel = "INFO";

        // Ensure log directory exists
        if( !string.IsNullOrEmpty( LogFileDirectory ) && !Directory.Exists( LogFileDirectory ) )
        {
            Directory.CreateDirectory( LogFileDirectory );
        }

        LoadConfig();
    }

    /// <summary>
    /// Loads configuration settings from the JSON file.
    /// </summary>
    public void LoadConfig()
    {
        try
        {
            if( !File.Exists( _configPath ) )
            {
                _loggingService?.LogDebug( "Configuration file not found, using defaults" );
                Save();
                return;
            }

            string json = File.ReadAllText( _configPath );
            var settings = JsonSerializer.Deserialize<AppConfiguration>( json );

            if( settings != null )
            {
                _loggingService?.LogDebug( "Loading configuration from file" );
                IsDarkTheme = settings.IsDarkTheme;
                LogFileDirectory = settings.LogFileDirectory ?? LogFileDirectory;
                MaxConsoleLines = settings.MaxConsoleLines;
                LogLevel = settings.LogLevel ?? LogLevel;
            }
        }
        catch( Exception ex )
        {
            _loggingService?.LogError( "Error loading configuration", ex );
        }
    }

    /// <summary>
    /// Saves configuration settings to the JSON file.
    /// </summary>
    public void Save()
    {
        try
        {
            _loggingService?.LogDebug( "Saving configuration to file" );
            string json = JsonSerializer.Serialize( this, new JsonSerializerOptions { WriteIndented = true } );
            File.WriteAllText( _configPath, json );
        }
        catch( Exception ex )
        {
            _loggingService?.LogError( "Error saving configuration", ex );
        }
    }
}