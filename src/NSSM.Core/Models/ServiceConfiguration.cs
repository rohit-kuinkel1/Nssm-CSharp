using NSSM.Core.Service.Enums;

namespace NSSM.Core.Models
{
    /// <summary>
    /// Represents a complete configuration for a Windows service managed by NSSM.
    /// This class centralizes all service parameters in one place to avoid scattered parameters and improve maintainability.
    /// </summary>
    public class ServiceConfiguration
    {
        /// <summary>
        /// Gets or sets the unique name of the service, used for identification in the service control manager
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user-friendly display name shown in the Windows service manager
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the service's purpose and functionality
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to the executable file that will run as a service
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command-line arguments to pass to the executable
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working directory for the service process
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service startup type (automatic, manual, disabled, etc.)
        /// </summary>
        public ServiceStartup StartupType { get; set; } = ServiceStartup.AutoStart;

        /// <summary>
        /// Gets or sets whether the service should be delayed when starting automatically
        /// </summary>
        public bool DelayedAutoStart { get; set; } = false;

        /// <summary>
        /// Gets or sets the username under which the service should run (null for LocalSystem)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password for the username (null for LocalSystem)
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the action to take when the service process exits
        /// </summary>
        public ExitAction OnExit { get; set; } = ExitAction.Restart;

        /// <summary>
        /// Gets or sets the priority at which the service process should run
        /// </summary>
        public ProcessPriority ProcessPriority { get; set; } = ProcessPriority.Normal;

        /// <summary>
        /// Gets or sets the number of milliseconds to delay after starting the service
        /// </summary>
        public int StartupDelayMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait for the service to stop gracefully
        /// </summary>
        public int StopTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait between restart attempts
        /// </summary>
        public int RestartThrottleMs { get; set; } = 1500;

        /// <summary>
        /// Gets or sets whether to rotate logs when the service starts
        /// </summary>
        public bool RotateLogsOnStart { get; set; } = false;

        /// <summary>
        /// Gets or sets the path where service logs should be stored
        /// </summary>
        public string? LogDirectory { get; set; }

        /// <summary>
        /// Gets or sets a list of service dependencies that must start before this service
        /// </summary>
        public string[]? Dependencies { get; set; }

        /// <summary>
        /// Creates a new service configuration with default values
        /// </summary>
        public ServiceConfiguration()
        {
        }

        /// <summary>
        /// Creates a new service configuration with essential values set
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="executablePath">The path to the executable</param>
        /// <param name="displayName">The display name (defaults to service name if null)</param>
        public ServiceConfiguration(
            string serviceName,
            string executablePath,
            string? displayName = null
        )
        {
            ServiceName = serviceName;
            ExecutablePath = executablePath;
            DisplayName = displayName ?? serviceName;
        }
    }
}