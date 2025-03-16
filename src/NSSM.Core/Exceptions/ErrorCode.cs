namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Defines all possible error codes used throughout the application.
    /// </summary>
    public enum ErrorCode
    {
        // General errors (1-99)
        Unknown = 1,
        Unexpected = 2,

        // Service-related errors (100-199)
        ServiceNotFound = 100,
        ServiceAlreadyExists = 101,
        ServiceStartFailure = 102,
        ServiceStopFailure = 103,
        ServiceInstallFailure = 104,
        ServiceRemovalFailure = 105,
        ServiceActionTimeout = 106,

        // Registry-related errors (200-299)
        RegistryAccessDenied = 200,
        RegistryKeyNotFound = 201,
        RegistryUpdateFailed = 202,

        // Process-related errors (300-399)
        ProcessStartFailure = 300,
        ProcessNotFound = 301,
        ProcessAccessDenied = 302,

        // Configuration errors (400-499)
        ConfigurationInvalid = 400,
        ConfigurationMissing = 401,
    }
}
