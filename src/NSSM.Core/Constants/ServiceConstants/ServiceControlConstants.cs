namespace NSSM.Core.Constants
{
    /// <summary>
    /// Service control codes for interacting with Windows services
    /// </summary>
    public static class ServiceControlConstants
    {
        /// <summary>
        /// Start the service
        /// </summary>
        public const int Start = 0x00000000;

        /// <summary>
        /// Stop the service
        /// </summary>
        public const int Stop = 0x00000001;

        /// <summary>
        /// Pause the service
        /// </summary>
        public const int Pause = 0x00000002;

        /// <summary>
        /// Continue a paused service
        /// </summary>
        public const int Continue = 0x00000003;

        /// <summary>
        /// Query the service status
        /// </summary>
        public const int Interrogate = 0x00000004;

        /// <summary>
        /// Notify service of system shutdown
        /// </summary>
        public const int Shutdown = 0x00000005;

        /// <summary>
        /// Rotate service logs
        /// </summary>
        public const int Rotate = 0x00000006;
    }
} 