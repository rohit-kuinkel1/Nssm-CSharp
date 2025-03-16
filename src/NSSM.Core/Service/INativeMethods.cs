namespace NSSM.Core.Service
{
    /// <summary>
    /// Interface for Windows native API calls related to service management
    /// </summary>
    public interface INativeMethods
    {
        /// <summary>
        /// Opens a connection to the service control manager
        /// </summary>
        IntPtr OpenSCManager(
            string? machineName,
            string? databaseName,
            uint desiredAccess
        );

        /// <summary>
        /// Creates a new Windows service
        /// </summary>
        IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string? lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string? lpDependencies,
            string? lpServiceStartName,
            string? lpPassword
        );

        /// <summary>
        /// Opens an existing service
        /// </summary>
        IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess
        );

        /// <summary>
        /// Closes a service handle
        /// </summary>
        bool CloseServiceHandle( IntPtr hSCObject );

        /// <summary>
        /// Deletes a service
        /// </summary>
        bool DeleteService( IntPtr hService );

        /// <summary>
        /// Sends a control code to a service
        /// </summary>
        bool ControlService(
            IntPtr hService,
            uint dwControl,
            ref ServiceStructures.SERVICE_STATUS lpServiceStatus
        );

        /// <summary>
        /// Changes the configuration of a service
        /// </summary>
        bool ChangeServiceConfig(
            IntPtr hService,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string? lpBinaryPathName,
            string? lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string? lpDependencies,
            string? lpServiceStartName,
            string? lpPassword,
            string? lpDisplayName
        );

        /// <summary>
        /// Changes the configuration of a service (extended)
        /// </summary>
        bool ChangeServiceConfig2(
            IntPtr hService,
            uint dwInfoLevel,
            IntPtr lpInfo
        );
    }
}