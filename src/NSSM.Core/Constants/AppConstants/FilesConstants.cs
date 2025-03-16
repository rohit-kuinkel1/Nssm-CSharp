namespace NSSM.Core.Constants.AppConstants
{
    /// <summary>
    /// File-related constants including paths and extensions
    /// </summary>
    public static class FileConstants
    {
        public const string LogFileDirectory = "%TEMP%"; // Using environment variable
        public const string DebugLogFileName = "nssm_wpf_debug.log";
        public const string ErrorLogFileName = "nssm_wpf_error.log";
    }
}
