namespace NSSM.Core.Logging
{
    /// <summary>
    /// Event arguments for log messages
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the timestamp of the log message
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the log level
        /// </summary>
        public string Level { get; }

        /// <summary>
        /// Gets the log message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception, if any
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Initializes a new instance of the LogEventArgs class
        /// </summary>
        public LogEventArgs(
            string level,
            string message,
            Exception? exception = null
        )
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
            Exception = exception;
        }
    }
}
