using NSSM.Core.Services;

namespace NSSM.Core.Messaging;

/// <summary>
/// Message sent when a service status has changed.
/// This allows different parts of the application to react to service status changes
/// without direct dependencies.
/// </summary>
public class ServiceStatusChangedMessage
{
    /// <summary>
    /// Gets the name of the service that had a status change
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the new status of the service
    /// </summary>
    public string NewStatus { get; }

    /// <summary>
    /// Gets the previous status of the service, if available
    /// </summary>
    public string? PreviousStatus { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceStatusChangedMessage class.
    /// </summary>
    /// <param name="serviceName">The name of the service</param>
    /// <param name="newStatus">The new status of the service</param>
    /// <param name="previousStatus">The previous status, if available</param>
    public ServiceStatusChangedMessage( string serviceName, string newStatus, string? previousStatus = null )
    {
        ServiceName = serviceName;
        NewStatus = newStatus;
        PreviousStatus = previousStatus;
    }
}

/// <summary>
/// Message sent when a service has been installed.
/// This allows different parts of the application to react to new service installations
/// without direct dependencies.
/// </summary>
public class ServiceInstalledMessage
{
    /// <summary>
    /// Gets the service information for the newly installed service
    /// </summary>
    public ServiceInfo ServiceInfo { get; }

    /// <summary>
    /// Gets the name of the service that was installed
    /// </summary>
    public string ServiceName => ServiceInfo.Name;

    /// <summary>
    /// Initializes a new instance of the ServiceInstalledMessage class.
    /// </summary>
    /// <param name="serviceInfo">Information about the installed service</param>
    public ServiceInstalledMessage( ServiceInfo serviceInfo )
    {
        ServiceInfo = serviceInfo;
    }
}

/// <summary>
/// Message sent when a service has been removed.
/// This allows different parts of the application to react to service removals
/// without direct dependencies.
/// </summary>
public class ServiceRemovedMessage
{
    /// <summary>
    /// Gets the name of the service that was removed
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceRemovedMessage class.
    /// </summary>
    /// <param name="serviceName">The name of the removed service</param>
    public ServiceRemovedMessage( string serviceName )
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Message sent when the service list should be refreshed.
/// This allows requesting a service list refresh from anywhere in the application
/// without direct dependencies.
/// </summary>
public class RefreshServicesRequestMessage
{
    /// <summary>
    /// Gets a value indicating whether the refresh should be forced even if
    /// the normal refresh interval hasn't elapsed.
    /// </summary>
    public bool Force { get; }

    /// <summary>
    /// Initializes a new instance of the RefreshServicesRequestMessage class.
    /// </summary>
    /// <param name="force">Whether to force the refresh</param>
    public RefreshServicesRequestMessage( bool force = false )
    {
        Force = force;
    }
}

/// <summary>
/// Message sent when a service has been changed.
/// This allows different parts of the application to react to service changes
/// without direct dependencies.
/// </summary>
public class ServiceChangedMessage
{
    /// <summary>
    /// Gets the name of the service that was changed
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceChangedMessage class.
    /// </summary>
    /// <param name="serviceName">The name of the changed service</param>
    public ServiceChangedMessage( string serviceName )
    {
        ServiceName = serviceName;
    }
}
