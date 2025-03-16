namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Base exception class for all application-specific exceptions.
    /// Provides standardized error codes and categorization to improve error handling and reporting.
    /// </summary>
    public class NssmException : Exception
    {
        /// <summary>
        /// Gets the error code associated with this exception
        /// </summary>
        public ErrorCode Code { get; }

        /// <summary>
        /// Gets the error category for grouping related errors
        /// </summary>
        public ErrorCategory Category { get; }

        /// <summary>
        /// Initializes a new instance of the AppException class with the specified error code.
        /// </summary>
        /// <param name="code">The error code that identifies this exception</param>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public NssmException(
            ErrorCode code,
            string message,
            Exception? innerException = null
        )
            : base( message, innerException )
        {
            Code = code;
            Category = DetermineCategory( code );
        }

        /// <summary>
        /// Determines the error category based on the error code.
        /// </summary>
        /// <param name="code">The error code to categorize</param>
        /// <returns>The appropriate error category</returns>
        private static ErrorCategory DetermineCategory( ErrorCode code )
        {
            // Categorize errors based on their code range
            return code switch
            {
                ErrorCode.ServiceNotFound or
                ErrorCode.ServiceStartFailure or
                ErrorCode.ServiceStopFailure or
                ErrorCode.ServiceActionTimeout => ErrorCategory.ServiceOperation,

                ErrorCode.RegistryAccessDenied or
                ErrorCode.RegistryKeyNotFound => ErrorCategory.RegistryOperation,

                ErrorCode.ProcessStartFailure or
                ErrorCode.ProcessAccessDenied => ErrorCategory.ProcessOperation,

                ErrorCode.ConfigurationInvalid or
                ErrorCode.ConfigurationMissing => ErrorCategory.Configuration,

                _ => ErrorCategory.General
            };
        }
    }
}