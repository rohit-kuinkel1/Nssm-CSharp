using Microsoft.Extensions.Logging;
using NSSM.Core.Constants.AppConstants;
namespace NSSM.Core.Logging
{
    /// <summary>
    /// Centralized logging service that provides consistent logging capabilities throughout the application.
    /// This service supports file-based logging, console output, and integration with Microsoft's ILogger.
    /// </summary>
    public class LoggingService
    {
        private readonly ILogger _logger;
        private readonly string _logFilePath;

        /// <summary>
        /// Event raised when a log message is emitted
        /// </summary>
        public event EventHandler<LogEventArgs>? LogMessageReceived;

        /// <summary>
        /// Initializes a new instance of the LoggingService class.
        /// </summary>
        /// <param name="logger">The Microsoft logger instance to use for logging</param>
        public LoggingService( ILogger<LoggingService> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );

            string tempPath = Path.GetTempPath();
            _logFilePath = Path.Combine( tempPath, FileConstants.DebugLogFileName );

            // Initialize log file
            EnsureLogFileExists();
        }

        /// <summary>
        /// Logs information level messages to both the ILogger and the log file.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional format arguments for the message</param>
        public void LogInformation( 
            string message, 
            params object[] args 
        )
        {
            string formattedMessage = args.Length > 0 ? string.Format( message, args ) : message;
            _logger.LogInformation( formattedMessage );
            WriteToLogFile( $"INFO: {formattedMessage}" );
            OnLogMessageReceived( "INFO", formattedMessage );
        }

        /// <summary>
        /// Logs warning level messages to both the ILogger and the log file.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional format arguments for the message</param>
        public void LogWarning( 
            string message,
            params object[] args
        )
        {
            string formattedMessage = args.Length > 0 ? string.Format( message, args ) : message;
            _logger.LogWarning( formattedMessage );
            WriteToLogFile( $"WARNING: {formattedMessage}" );
            OnLogMessageReceived( "WARNING", formattedMessage );
        }

        /// <summary>
        /// Logs debug level messages to both the ILogger and the log file.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional format arguments for the message</param>
        public void LogDebug( 
            string message, 
            params object[] args
        )
        {
            string formattedMessage = args.Length > 0 ? string.Format( message, args ) : message;
            _logger.LogDebug( formattedMessage );
            WriteToLogFile( $"DEBUG: {formattedMessage}" );
            OnLogMessageReceived( "DEBUG", formattedMessage );
        }

        /// <summary>
        /// Logs error level messages to both the ILogger and the log file.
        /// </summary>
        /// <param name="message">The error message to log</param>
        /// <param name="ex">Optional exception that caused the error</param>
        public void LogError( 
            string message, 
            Exception? ex = null
        )
        {
            _logger.LogError( ex, message );

            string logMessage = $"ERROR: {message}";
            if( ex != null )
            {
                logMessage += $" | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";

                if( ex.InnerException != null )
                {
                    logMessage += $" | Inner: {ex.InnerException.Message}";
                }
            }

            WriteToLogFile( logMessage );
            OnLogMessageReceived( "ERROR", message, ex );
        }

        /// <summary>
        /// Raises the LogMessageReceived event
        /// </summary>
        protected virtual void OnLogMessageReceived( 
            string level, 
            string message, 
            Exception? exception = null
        )
        {
            LogMessageReceived?.Invoke( this, new LogEventArgs( level, message, exception ) );
        }

        /// <summary>
        /// Writes a message to the application log file with timestamp.
        /// </summary>
        /// <param name="message">The message to write to the log file</param>
        private void WriteToLogFile( string message )
        {
            try
            {
                using( StreamWriter writer = File.AppendText( _logFilePath ) )
                {
                    writer.WriteLine( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}" );
                }
            }
            catch
            {
                //suppress any logging errors to prevent cascade failures
                //if logging fails, the application should still continue
            }
        }

        /// <summary>
        /// Ensures that the log file exists and creates it if it doesn't.
        /// </summary>
        private void EnsureLogFileExists()
        {
            try
            {
                if( !File.Exists( _logFilePath ) )
                {
                    using( StreamWriter writer = File.CreateText( _logFilePath ) )
                    {
                        writer.WriteLine( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - LOG INITIALIZED" );
                    }
                }
            }
            catch
            {
                //if we can't create the log file, we simply continue without logging to file
            }
        }
    }
}