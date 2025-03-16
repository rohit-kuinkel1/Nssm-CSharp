using System.Runtime.InteropServices;

namespace NSSM.Core.Service
{
    /// <summary>
    /// Contains structures used for Windows service management
    /// </summary>
    public static class ServiceStructures
    {
        /// <summary>
        /// Contains status information for a service
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        public struct SERVICE_STATUS
        {
            /// <summary>
            /// The type of service
            /// </summary>
            public uint dwServiceType;

            /// <summary>
            /// The current state of the service
            /// </summary>
            public uint dwCurrentState;

            /// <summary>
            /// The control codes the service accepts
            /// </summary>
            public uint dwControlsAccepted;

            /// <summary>
            /// The exit code returned by the service
            /// </summary>
            public uint dwWin32ExitCode;

            /// <summary>
            /// A service-specific error code
            /// </summary>
            public uint dwServiceSpecificExitCode;

            /// <summary>
            /// A checkpoint value that the service increments periodically to report its progress
            /// </summary>
            public uint dwCheckPoint;

            /// <summary>
            /// The estimated time required for a pending start, stop, pause, or continue operation, in milliseconds
            /// </summary>
            public uint dwWaitHint;
        }

        //scm constants
        public const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
        public const uint SERVICE_ALL_ACCESS = 0xF01FF;
        public const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public const uint SERVICE_ERROR_NORMAL = 0x00000001;
        public const uint SERVICE_AUTO_START = 0x00000002;
        public const uint SERVICE_DEMAND_START = 0x00000003;
        public const uint SERVICE_DISABLED = 0x00000004;
        public const uint SERVICE_CONTROL_STOP = 0x00000001;
        public const uint SERVICE_CONTROL_PAUSE = 0x00000002;
        public const uint SERVICE_CONTROL_CONTINUE = 0x00000003;
    }
}