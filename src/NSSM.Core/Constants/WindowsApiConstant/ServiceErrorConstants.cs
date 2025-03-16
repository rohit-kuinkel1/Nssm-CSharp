namespace NSSM.Core.Constants.WindowsApiConstant
{
    /// <summary>
    /// Constants for service error control
    /// </summary>
    public static class ServiceErrorConstants
    {
        /// <summary>
        /// The startup program ignores the error and continues the startup operation
        /// </summary>
        public const uint Ignore = 0x00000000;

        /// <summary>
        /// The startup program logs the error in the event log but continues the startup operation
        /// </summary>
        public const uint Normal = 0x00000001;

        /// <summary>
        /// The startup program logs the error in the event log. If the last-known-good configuration is being started, the startup operation continues. Otherwise, the system is restarted with the last-known-good configuration
        /// </summary>
        public const uint Severe = 0x00000002;

        /// <summary>
        /// The startup program logs the error in the event log, if possible. If the last-known-good configuration is being started, the startup operation fails. Otherwise, the system is restarted with the last-known-good configuration
        /// </summary>
        public const uint Critical = 0x00000003;
    }
}
