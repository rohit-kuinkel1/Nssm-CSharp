using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSM.CLI.Constants;
using NSSM.Core.Service;

namespace NSSM.CLI.Service
{
    /// <summary>
    /// Handles execution when NSSM is running in service mode
    /// </summary>
    public class ServiceModeHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceModeHandler> _logger;

        public ServiceModeHandler( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<ServiceModeHandler>>();
        }

        /// <summary>
        /// Runs NSSM in service mode with the specified arguments
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Exit code (0 for success, non-zero for error)</returns>
        public int Run( string[] args )
        {
            try
            {
                WriteServiceStartupLog( args );

                var serviceArgs = args.Length > 1
                    ? new string[] { args[1].Trim( '"' ) }
                    : Array.Empty<string>();

                if( serviceArgs.Length > 0 )
                {
                    _logger.LogInformation( "Running as service: {ServiceName}", serviceArgs[0] );
                    WriteServiceDebugLog( serviceArgs[0], args );
                }

                try
                {
                    _logger.LogInformation( "Starting service with ServiceProgram.RunAsService" );
                    ServiceProgram.RunAsService( serviceArgs );
                    return 0;
                }
                catch( Exception ex ) //catch all
                {
                    _logger.LogError( ex, "Error in ServiceProgram.RunAsService" );
                    return TryFallbackServiceImplementation( serviceArgs );
                }
            }
            catch( Exception ex ) //catch all
            {
                Console.Error.WriteLine( $"Error in service mode: {ex.Message}" );
                return 1;
            }
        }

        /// <summary>
        /// Attempts to use a fallback service implementation if the primary method fails
        /// </summary>
        private int TryFallbackServiceImplementation( string[] serviceArgs )
        {
            try
            {
                _logger.LogInformation( "Falling back to direct service implementation" );

                string serviceName = serviceArgs.Length > 0 ? serviceArgs[0] : "NSSM";

                if( !Environment.UserInteractive )
                {
                    //non-interactive mode (actual service)
                    var directServiceHost = new NssmServiceHost(
                        serviceName,
                        _serviceProvider.GetRequiredService<ILogger<NssmServiceHost>>()
                    );

                    System.ServiceProcess.ServiceBase.Run( new System.ServiceProcess.ServiceBase[] { directServiceHost } );
                }
                else
                {
                    //interactive debugging mode
                    RunServiceInInteractiveMode( serviceName );
                }

                return 0;
            }
            catch( Exception innerEx )
            {
                _logger.LogError( innerEx, "Error in direct service implementation fallback" );
                return 1;
            }
        }

        /// <summary>
        /// Runs the service in interactive mode for debugging purposes
        /// </summary>
        private void RunServiceInInteractiveMode( string serviceName )
        {
            _logger.LogInformation( "Running in interactive mode with direct method calls" );

            var directServiceHost = new NssmServiceHost( serviceName,
                _serviceProvider.GetRequiredService<ILogger<NssmServiceHost>>() );

            typeof( System.ServiceProcess.ServiceBase )
                .GetMethod( "OnStart", BindingFlags.NonPublic | BindingFlags.Instance )
                ?.Invoke( directServiceHost, new object[] { Array.Empty<string>() } );

            _logger.LogInformation( "Service started directly. Press any key to stop." );
            Console.ReadKey();

            typeof( System.ServiceProcess.ServiceBase )
                .GetMethod( "OnStop", BindingFlags.NonPublic | BindingFlags.Instance )
                ?.Invoke( directServiceHost, null );

            _logger.LogInformation( "Service stopped." );
        }

        /// <summary>
        /// Writes a startup log to help diagnose service startup issues
        /// </summary>
        private void WriteServiceStartupLog( string[] args )
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
                    CliConstants.LogDirectory,
                    CliConstants.ServiceStartupLogFilename
                );

                var logDir = Path.GetDirectoryName( logPath );
                if( !string.IsNullOrEmpty( logDir ) )
                {
                    Directory.CreateDirectory( logDir );
                }

                File.AppendAllText(
                    logPath,
                    $"[{DateTime.Now}] Service startup initiated\r\n" +
                    $"Args: {string.Join( " ", args )}\r\n" +
                    $"Current executable: {System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "unknown"}\r\n" +
                    $"Directory: {Directory.GetCurrentDirectory()}\r\n"
                );
            }
            catch( Exception ex ) //catch all
            {
                _logger.LogWarning( "Error writing startup log: {Error}", ex.Message );
            }
        }

        /// <summary>
        /// Writes a debug log with service-specific information
        /// </summary>
        private void WriteServiceDebugLog(
            string serviceName,
            string[] args
        )
        {
            try
            {
                var debugPath = Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
                    CliConstants.LogDirectory,
                    CliConstants.ServiceDebugLogFilename
                );

                var debugDir = Path.GetDirectoryName( debugPath );
                if( !string.IsNullOrEmpty( debugDir ) )
                {
                    Directory.CreateDirectory( debugDir );

                    File.AppendAllText(
                        debugPath,
                        $"[{DateTime.Now}] Starting service: {serviceName}\r\n" +
                        $"Args: {string.Join( " ", args )}\r\n" +
                        $"Current executable: {System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "unknown"}\r\n"
                    );
                }
            }
            catch( Exception ex ) //catch all
            {
                _logger.LogWarning( "Error writing debug log: {Error}", ex.Message );
            }
        }
    }
}