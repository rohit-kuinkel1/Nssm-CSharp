namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Base exception for all service-related errors.
    /// This class provides a foundation for more specific service exceptions.
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// Gets the name of the service that caused the exception
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the error code, if applicable
        /// </summary>
        public int? ErrorCode { get; }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="message">The error message</param>
        public ServiceException( string message )
            : base( message )
        {
            ServiceName = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceException(
            string message,
            Exception innerException
        )
            : base( message, innerException )
        {
            ServiceName = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="serviceName">The name of the service that caused the exception</param>
        /// <param name="message">The error message</param>
        public ServiceException(
            string serviceName,
            string message
        )
            : base( message )
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="serviceName">The name of the service that caused the exception</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceException(
            string serviceName,
            string message,
            Exception innerException
        )
            : base( message, innerException )
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="serviceName">The name of the service that caused the exception</param>
        /// <param name="errorCode">The error code that occurred</param>
        /// <param name="message">The error message</param>
        public ServiceException(
            string serviceName,
            int errorCode,
            string message
        )
            : base( message )
        {
            ServiceName = serviceName;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a new instance of the ServiceException class
        /// </summary>
        /// <param name="serviceName">The name of the service that caused the exception</param>
        /// <param name="errorCode">The error code that occurred</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceException(
            string serviceName,
            int errorCode,
            string message,
            Exception innerException
        )
            : base( message, innerException )
        {
            ServiceName = serviceName;
            ErrorCode = errorCode;
        }
    }
}