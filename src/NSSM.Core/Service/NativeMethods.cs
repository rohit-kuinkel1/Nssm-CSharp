using System.Runtime.InteropServices;

namespace NSSM.Core.Service
{
    /// <summary>
    /// Native Windows API methods for service management
    /// </summary>
    internal static class NativeMethods
    {
        // Service Control Manager access rights
        public const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
        public const uint SC_MANAGER_CREATE_SERVICE = 0x0002;
        public const uint SC_MANAGER_CONNECT = 0x0001;
        public const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        public const uint SC_MANAGER_LOCK = 0x0008;
        public const uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;
        public const uint SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020;

        // Service access rights
        public const uint SERVICE_ALL_ACCESS = 0xF01FF;
        public const uint SERVICE_CHANGE_CONFIG = 0x0002;
        public const uint SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
        public const uint SERVICE_INTERROGATE = 0x0080;
        public const uint SERVICE_PAUSE_CONTINUE = 0x0040;
        public const uint SERVICE_QUERY_CONFIG = 0x0001;
        public const uint SERVICE_QUERY_STATUS = 0x0004;
        public const uint SERVICE_START = 0x0010;
        public const uint SERVICE_STOP = 0x0020;
        public const uint SERVICE_USER_DEFINED_CONTROL = 0x0100;

        // Service types
        public const uint SERVICE_KERNEL_DRIVER = 0x00000001;
        public const uint SERVICE_FILE_SYSTEM_DRIVER = 0x00000002;
        public const uint SERVICE_ADAPTER = 0x00000004;
        public const uint SERVICE_RECOGNIZER_DRIVER = 0x00000008;
        public const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public const uint SERVICE_WIN32_SHARE_PROCESS = 0x00000020;
        public const uint SERVICE_INTERACTIVE_PROCESS = 0x00000100;

        // Service start types
        public const uint SERVICE_BOOT_START = 0x00000000;
        public const uint SERVICE_SYSTEM_START = 0x00000001;
        public const uint SERVICE_AUTO_START = 0x00000002;
        public const uint SERVICE_DEMAND_START = 0x00000003;
        public const uint SERVICE_DISABLED = 0x00000004;

        // Service error control
        public const uint SERVICE_ERROR_IGNORE = 0x00000000;
        public const uint SERVICE_ERROR_NORMAL = 0x00000001;
        public const uint SERVICE_ERROR_SEVERE = 0x00000002;
        public const uint SERVICE_ERROR_CRITICAL = 0x00000003;

        // Service config
        public const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        public const uint SERVICE_CONFIG_DESCRIPTION = 1;
        public const uint SERVICE_CONFIG_FAILURE_ACTIONS = 2;
        public const uint SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;
        public const uint SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 4;
        public const uint SERVICE_CONFIG_SERVICE_SID_INFO = 5;
        public const uint SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 6;
        public const uint SERVICE_CONFIG_PRESHUTDOWN_INFO = 7;
        public const uint SERVICE_CONFIG_TRIGGER_INFO = 8;
        public const uint SERVICE_CONFIG_PREFERRED_NODE = 9;

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr OpenSCManager(
            string? lpMachineName,
            string? lpDatabaseName,
            uint dwDesiredAccess
        );

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr CreateService(
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

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess
        );

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CloseServiceHandle( IntPtr hSCObject );

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool DeleteService( IntPtr hService );

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ControlService(
            IntPtr hService,
            uint dwControl,
            ref ServiceStructures.SERVICE_STATUS lpServiceStatus
        );

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ChangeServiceConfig(
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

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ChangeServiceConfig2(
            IntPtr hService,
            uint dwInfoLevel,
            IntPtr lpInfo
        );

        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct SERVICE_DESCRIPTION
        {
            public string? lpDescription;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct SERVICE_DELAYED_AUTO_START_INFO
        {
            [MarshalAs( UnmanagedType.Bool )]
            public bool fDelayedAutostart;
        }
    }
}