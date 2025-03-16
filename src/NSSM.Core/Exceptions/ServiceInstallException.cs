namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a service installation operation fails.
    /// </summary>
    public class ServiceInstallException : ServiceException
    {
        /// <summary>
        /// Gets the path to the executable that failed to install
        /// </summary>
        public string ExecutablePath { get; }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="message">The error message</param>
        public ServiceInstallException(
            string serviceName,
            string message
        )
            : base( serviceName, message )
        {
            ExecutablePath = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="executablePath">The path to the executable that failed to install</param>
        /// <param name="message">The error message</param>
        public ServiceInstallException(
            string serviceName,
            string executablePath,
            string message
        )
            : base( serviceName, message )
        {
            ExecutablePath = executablePath;
        }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="errorCode">The Win32 error code</param>
        /// <param name="message">The error message</param>
        public ServiceInstallException(
            string serviceName,
            int errorCode,
            string message
        )
            : base( serviceName, errorCode, message )
        {
            ExecutablePath = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="executablePath">The path to the executable that failed to install</param>
        /// <param name="errorCode">The Win32 error code</param>
        /// <param name="message">The error message</param>
        public ServiceInstallException(
            string serviceName,
            string executablePath,
            int errorCode,
            string message
        )
            : base( serviceName, errorCode, message )
        {
            ExecutablePath = executablePath;
        }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceInstallException(
            string serviceName,
            string message,
            Exception innerException
        )
            : base( serviceName, message, innerException )
        {
            ExecutablePath = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the ServiceInstallException class
        /// </summary>
        /// <param name="serviceName">The name of the service that failed to install</param>
        /// <param name="executablePath">The path to the executable that failed to install</param>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception that caused this exception</param>
        public ServiceInstallException(
            string serviceName,
            string executablePath,
            string message,
            Exception innerException
        )
            : base( serviceName, message, innerException )
        {
            ExecutablePath = executablePath;
        }
    }
}