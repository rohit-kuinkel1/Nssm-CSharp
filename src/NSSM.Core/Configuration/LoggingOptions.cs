namespace NSSM.Core.Configuration
{

    /// <summary>
    /// Options for configuring the logging behavior.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Gets or sets the minimum log level.
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Gets or sets whether to log to file.
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;

        /// <summary>
        /// Gets or sets the log file directory.
        /// </summary>
        public string LogFileDirectory { get; set; } = "%TEMP%";
    }
}
