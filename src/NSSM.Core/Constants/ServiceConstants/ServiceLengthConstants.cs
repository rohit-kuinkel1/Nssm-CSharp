namespace NSSM.Core.Constants
{
    /// <summary>
    /// Length constants for various service properties
    /// </summary>
    public static class ServiceLengthConstants
    {
        /// <summary>
        /// Maximum length of a service name
        /// </summary>
        public const int ServiceName = 256;

        /// <summary>
        /// Maximum length of an executable path (equivalent to PATH_LENGTH in C++)
        /// </summary>
        public const int ExecutablePath = 260;

        /// <summary>
        /// Maximum length of command arguments
        /// </summary>
        public const int CommandLine = 32768;

        /// <summary>
        /// Maximum length of a registry key
        /// </summary>
        public const int RegistryKey = 255;

        /// <summary>
        /// Maximum length of a registry value
        /// </summary>
        public const int RegistryValue = 16383;

        /// <summary>
        /// Maximum length of a directory path (equivalent to DIR_LENGTH in C++)
        /// </summary>
        public const int Directory = 248;
    }
}