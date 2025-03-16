namespace NSSM.Core.Constants
{
    /// <summary>
    /// Default delay values for various service operations
    /// </summary>
    public static class ServiceDelayConstants
    {
        /// <summary>
        /// Default delay before throttling service restarts (milliseconds)
        /// </summary>
        public const int ResetThrottleRestart = 1500;

        /// <summary>
        /// Default delay before killing console processes (milliseconds)
        /// </summary>
        public const int KillConsole = 1500;

        /// <summary>
        /// Default delay before killing windowed processes (milliseconds)
        /// </summary>
        public const int KillWindow = 1500;

        /// <summary>
        /// Default delay before forcibly terminating process threads (milliseconds)
        /// </summary>
        public const int KillThreads = 1000;

        /// <summary>
        /// Default delay before starting a service (milliseconds)
        /// </summary>
        public const int Startup = 0;
    }
} 