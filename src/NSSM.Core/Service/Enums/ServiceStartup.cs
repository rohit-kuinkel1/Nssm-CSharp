namespace NSSM.Core.Service.Enums
{
    /// <summary>
    /// Service startup types
    /// </summary>
    public enum ServiceStartup
    {
        /// <summary>
        /// Service starts automatically during system startup
        /// </summary>
        AutoStart = 0,

        /// <summary>
        /// Service starts automatically after other services (delayed start)
        /// </summary>
        DelayedAutoStart = 1,

        /// <summary>
        /// Service starts only when manually started
        /// </summary>
        DemandStart = 2,

        /// <summary>
        /// Service is disabled and cannot be started
        /// </summary>
        Disabled = 3
    }
}