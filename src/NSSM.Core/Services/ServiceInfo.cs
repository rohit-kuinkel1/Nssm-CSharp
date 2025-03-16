namespace NSSM.Core.Services
{
    /// <summary>
    /// Contains essential information about a Windows service.
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// Gets or sets the name of the service
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the service
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the service
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the startup type of the service
        /// </summary>
        public string StartType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to the executable that runs the service
        /// </summary>
        public string BinaryPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account under which the service runs
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the service
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
