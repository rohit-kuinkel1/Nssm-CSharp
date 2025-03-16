using System.ServiceProcess;

namespace NSSM.WPF.Models;

public class ServiceModel
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StartupType { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;

    public static ServiceModel FromServiceController(
        ServiceController sc,
        string? executablePath = null,
        string? arguments = null,
        string? workingDirectory = null
    )
    {
        return new ServiceModel
        {
            ServiceName = sc.ServiceName,
            DisplayName = sc.DisplayName,
            Status = sc.Status.ToString(),
            StartupType = GetStartupType( sc.ServiceName ),
            ExecutablePath = executablePath ?? string.Empty,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory ?? string.Empty,
            Description = GetServiceDescription( sc.ServiceName )
        };
    }

    private static string GetStartupType( string serviceName )
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( $@"SYSTEM\CurrentControlSet\Services\{serviceName}" );
            if( key != null )
            {
                var startType = (int?)key.GetValue( "Start" );
                return startType switch
                {
                    2 => "Automatic",
                    3 => "Manual",
                    4 => "Disabled",
                    _ => "Unknown"
                };
            }
        }
        catch( Exception )
        {
            //ignore errors
        }

        return "Unknown";
    }

    private static string GetServiceDescription( string serviceName )
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( $@"SYSTEM\CurrentControlSet\Services\{serviceName}" );
            if( key != null )
            {
                return (string?)key.GetValue( "Description" ) ?? string.Empty;
            }
        }
        catch( Exception )
        {
            //ignore errors
        }

        return string.Empty;
    }
}
