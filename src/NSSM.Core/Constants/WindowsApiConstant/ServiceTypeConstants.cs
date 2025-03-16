namespace NSSM.Core.Constants.WindowsApiConstant
{
    /// <summary>
    /// Constants for service types
    /// </summary>
    public static class ServiceTypeConstants
    {
        /// <summary>
        /// Service that runs in its own process
        /// </summary>
        public const uint Win32OwnProcess = 0x00000010;

        /// <summary>
        /// Service that shares a process with other services
        /// </summary>
        public const uint Win32ShareProcess = 0x00000020;

        /// <summary>
        /// Service that can interact with the desktop
        /// </summary>
        public const uint InteractiveProcess = 0x00000100;
    }
}
