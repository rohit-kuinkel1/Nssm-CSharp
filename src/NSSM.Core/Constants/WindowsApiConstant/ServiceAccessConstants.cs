namespace NSSM.Core.Constants.WindowsApiConstant
{
    /// <summary>
    /// Constants for service-specific access rights
    /// </summary>
    public static class ServiceAccessConstants
    {
        /// <summary>
        /// All possible access rights to the service
        /// </summary>
        public const uint AllAccess = 0xF01FF;

        /// <summary>
        /// Required to call the QueryServiceConfig and QueryServiceConfig2 functions to query the service configuration
        /// </summary>
        public const uint QueryConfig = 0x0001;

        /// <summary>
        /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function to change the service configuration
        /// </summary>
        public const uint ChangeConfig = 0x0002;

        /// <summary>
        /// Required to call the QueryServiceStatus function to ask the service control manager about the status of the service
        /// </summary>
        public const uint QueryStatus = 0x0004;

        /// <summary>
        /// Required to call the EnumDependentServices function to enumerate all the services dependent on the service
        /// </summary>
        public const uint EnumerateDependents = 0x0008;

        /// <summary>
        /// Required to call the StartService function to start the service
        /// </summary>
        public const uint Start = 0x0010;

        /// <summary>
        /// Required to call the ControlService function to stop the service
        /// </summary>
        public const uint Stop = 0x0020;

        /// <summary>
        /// Required to call the ControlService function to pause or continue the service
        /// </summary>
        public const uint PauseContinue = 0x0040;

        /// <summary>
        /// Required to call the ControlService function to interrogate the service
        /// </summary>
        public const uint Interrogate = 0x0080;

        /// <summary>
        /// Required to call the ControlService function to specify a user-defined control code
        /// </summary>
        public const uint UserDefinedControl = 0x0100;

        /// <summary>
        /// Required to call the DeleteService function to delete the service
        /// </summary>
        public const uint Delete = 0x00010000;
    }

}
