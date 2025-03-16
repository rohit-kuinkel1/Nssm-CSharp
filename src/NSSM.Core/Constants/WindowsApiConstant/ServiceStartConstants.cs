namespace NSSM.Core.Constants.WindowsApiConstant
{
    /// <summary>
    /// Constants for service start types
    /// </summary>
    public static class ServiceStartConstants
    {
        /// <summary>
        /// A service started automatically by the service control manager during system startup
        /// </summary>
        public const uint AutoStart = 0x00000002;

        /// <summary>
        /// A service started by the service control manager when a process calls the StartService function
        /// </summary>
        public const uint DemandStart = 0x00000003;

        /// <summary>
        /// A service that cannot be started
        /// </summary>
        public const uint Disabled = 0x00000004;
    }

}
