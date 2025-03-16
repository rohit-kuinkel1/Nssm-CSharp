namespace NSSM.CLI.Constants
{
    /// <summary>
    /// Contains all constants used by the CLI application
    /// </summary>
    public static class CliConstants
    {
        /// <summary>
        /// Command-line argument to run NSSM as a service
        /// </summary>
        public const string RunAsServiceArg = "--run-as-service";

        /// <summary>
        /// Default log directory for service logging
        /// </summary>
        public const string LogDirectory = "NSSM";

        /// <summary>
        /// Default log filename for service startup debugging
        /// </summary>
        public const string ServiceStartupLogFilename = "service_startup.log";

        /// <summary>
        /// Default log filename for service debugging
        /// </summary>
        public const string ServiceDebugLogFilename = "service_debug.log";
    }
}