namespace NSSM.Core.Service
{
    /// <summary>
    /// Implementation of INativeMethods that calls actual Windows APIs
    /// </summary>
    public class WindowsNativeMethods : INativeMethods
    {
        public IntPtr OpenSCManager(
            string? machineName,
            string? databaseName,
            uint desiredAccess )
        {
            return NativeMethods.OpenSCManager( machineName, databaseName, desiredAccess );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <param name="lpServiceName"></param>
        /// <param name="lpDisplayName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwServiceType"></param>
        /// <param name="dwStartType"></param>
        /// <param name="dwErrorControl"></param>
        /// <param name="lpBinaryPathName"></param>
        /// <param name="lpLoadOrderGroup"></param>
        /// <param name="lpdwTagId"></param>
        /// <param name="lpDependencies"></param>
        /// <param name="lpServiceStartName"></param>
        /// <param name="lpPassword"></param>
        /// <returns></returns>
        public IntPtr CreateService(
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
        )
        {
            return NativeMethods.CreateService(
                hSCManager,
                lpServiceName,
                lpDisplayName,
                dwDesiredAccess,
                dwServiceType,
                dwStartType,
                dwErrorControl,
                lpBinaryPathName,
                lpLoadOrderGroup,
                lpdwTagId,
                lpDependencies,
                lpServiceStartName,
                lpPassword );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <param name="lpServiceName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        public IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess
        )
        {
            return NativeMethods.OpenService( hSCManager, lpServiceName, dwDesiredAccess );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hSCObject"></param>
        /// <returns></returns>
        public bool CloseServiceHandle( IntPtr hSCObject )
        {
            return NativeMethods.CloseServiceHandle( hSCObject );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hService"></param>
        /// <returns></returns>
        public bool DeleteService( IntPtr hService )
        {
            return NativeMethods.DeleteService( hService );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwControl"></param>
        /// <param name="lpServiceStatus"></param>
        /// <returns></returns>
        public bool ControlService(
            IntPtr hService,
            uint dwControl,
            ref ServiceStructures.SERVICE_STATUS lpServiceStatus
        )
        {
            return NativeMethods.ControlService( hService, dwControl, ref lpServiceStatus );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwServiceType"></param>
        /// <param name="dwStartType"></param>
        /// <param name="dwErrorControl"></param>
        /// <param name="lpBinaryPathName"></param>
        /// <param name="lpLoadOrderGroup"></param>
        /// <param name="lpdwTagId"></param>
        /// <param name="lpDependencies"></param>
        /// <param name="lpServiceStartName"></param>
        /// <param name="lpPassword"></param>
        /// <param name="lpDisplayName"></param>
        /// <returns></returns>
        public bool ChangeServiceConfig(
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
        )
        {
            return NativeMethods.ChangeServiceConfig(
                hService,
                dwServiceType,
                dwStartType,
                dwErrorControl,
                lpBinaryPathName,
                lpLoadOrderGroup,
                lpdwTagId,
                lpDependencies,
                lpServiceStartName,
                lpPassword,
                lpDisplayName );
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="dwInfoLevel"></param>
        /// <param name="lpInfo"></param>
        /// <returns></returns>
        public bool ChangeServiceConfig2(
            IntPtr hService,
            uint dwInfoLevel,
            IntPtr lpInfo
        )
        {
            return NativeMethods.ChangeServiceConfig2(
                hService,
                dwInfoLevel,
                lpInfo );
        }
    }
}