namespace NSSM.Core.Service.Enums
{
    /// <summary>
    /// Exit action types for handling service process termination
    /// </summary>
    public enum ExitAction
    {
        /// <summary>
        /// Restart the service
        /// </summary>
        Restart = 0,

        /// <summary>
        /// Ignore the exit and do nothing
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Exit the service
        /// </summary>
        Exit = 2,

        /// <summary>
        /// Terminate the service management process itself
        /// </summary>
        Suicide = 3
    }
}