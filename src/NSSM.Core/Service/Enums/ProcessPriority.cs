namespace NSSM.Core.Service.Enums
{
    /// <summary>
    /// Process priority classes
    /// </summary>
    public enum ProcessPriority
    {
        /// <summary>
        /// Highest priority level, for time-critical tasks
        /// </summary>
        Realtime = 0,

        /// <summary>
        /// High priority level
        /// </summary>
        High = 1,

        /// <summary>
        /// Priority level above normal
        /// </summary>
        AboveNormal = 2,

        /// <summary>
        /// Normal (default) priority level
        /// </summary>
        Normal = 3,

        /// <summary>
        /// Priority level below normal
        /// </summary>
        BelowNormal = 4,

        /// <summary>
        /// Lowest priority level
        /// </summary>
        Idle = 5
    }
}