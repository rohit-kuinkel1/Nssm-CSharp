namespace NSSM.Core.Constants.WindowsApiConstant
{
    /// <summary>
    /// Constants for service control manager access rights
    /// </summary>
    public static class ScmAccessConstants
    {
        /// <summary>
        /// All possible access rights to the service control manager
        /// </summary>
        public const uint AllAccess = 0xF003F;

        /// <summary>
        /// Required to connect to the service control manager
        /// </summary>
        public const uint Connect = 0x0001;

        /// <summary>
        /// Required to call the CreateService function to create a service object and add it to the database
        /// </summary>
        public const uint CreateService = 0x0002;

        /// <summary>
        /// Required to call the EnumServicesStatus or EnumServicesStatusEx function to list the services in the database
        /// </summary>
        public const uint EnumerateService = 0x0004;

        /// <summary>
        /// Required to call the LockServiceDatabase function to acquire a lock on the database
        /// </summary>
        public const uint Lock = 0x0008;

        /// <summary>
        /// Required to call the QueryServiceLockStatus function to retrieve the lock status information for the database
        /// </summary>
        public const uint QueryLockStatus = 0x0010;

        /// <summary>
        /// Required to call the NotifyBootConfigStatus function
        /// </summary>
        public const uint ModifyBootConfig = 0x0020;
    }
}
