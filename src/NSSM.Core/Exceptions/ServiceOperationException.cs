namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a general service operation (start, stop, etc.) fails.
    /// </summary>
    public class ServiceOperationException : ServiceException
    {
        /// <summary>
        /// Gets the type of operation that failed
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Creates a new instance of the ServiceOperationException class
        /// </summary>
        /// <param name="serviceName">The name of the service that had an operation failure</param>
        /// <param name="operation">The operation that failed (start, stop, etc.)</param>
        /// <param name="message">The error message</param>
        public ServiceOperationException(
            string serviceName,
            string operation,
            string message
        )
            : base( serviceName, message )
        {
            Operation = operation;
        }

        /// <summary>
        /// Creates a new instance of the ServiceOperationException class
        /// </summary>
        /// <param name="serviceName">The name of the service that had an operation failure</param>
        /// <param name="operation">The operation that failed (start, stop, etc.)</param>
        /// <param name="errorCode">The Win32 error code</param>
        /// <param name="message">The error message</param>
        public ServiceOperationException(
            string serviceName,
            string operation,
            int errorCode,
            string message
        )
            : base( serviceName, errorCode, message )
        {
            Operation = operation;
        }

        /// <summary>
        /// Creates a new instance of the ServiceOperationException class
        /// </summary>
        /// <param name="serviceName">The name of the service that had an operation failure</param>
        /// <param name="operation">The operation that failed (start, stop, etc.)</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceOperationException(
            string serviceName,
            string operation,
            string message,
            Exception innerException
        )
            : base( serviceName, message, innerException )
        {
            Operation = operation;
        }

        /// <summary>
        /// Creates a new instance of the ServiceOperationException class
        /// </summary>
        /// <param name="serviceName">The name of the service that had an operation failure</param>
        /// <param name="operation">The operation that failed (start, stop, etc.)</param>
        /// <param name="errorCode">The Win32 error code</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceOperationException(
            string serviceName,
            string operation,
            int errorCode,
            string message,
            Exception innerException
        )
            : base( serviceName, errorCode, message, innerException )
        {
            Operation = operation;
        }
    }
}