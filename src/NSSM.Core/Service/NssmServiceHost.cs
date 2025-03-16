using System.ServiceProcess;
using Microsoft.Extensions.Logging;

namespace NSSM.Core.Service
{
    /// <summary>
    /// Windows service implementation that hosts and manages an application process
    /// </summary>
    public class NssmServiceHost : ServiceBase
    {
        private readonly ILogger<NssmServiceHost> _logger;
        private System.Diagnostics.Process? _managedProcess;
        private string _serviceName;
        private CancellationTokenSource? _cts;

        public NssmServiceHost(
            string serviceName,
            ILogger<NssmServiceHost> logger
        )
        {
            _serviceName = serviceName;
            _logger = logger;
            ServiceName = serviceName;
        }

        protected override void OnStart( string[] args )
        {
            _logger.LogInformation( $"Starting service {_serviceName}" );
            _cts = new CancellationTokenSource();

            try
            {
                //read application path and arguments from registry
                string? executablePath = null;
                string? arguments = null;
                string? workingDirectory = null;

                try
                {
                    using var parametersKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        $"SYSTEM\\CurrentControlSet\\Services\\{_serviceName}\\Parameters" );
                    if( parametersKey != null )
                    {
                        executablePath = parametersKey.GetValue( "Application" ) as string;
                        arguments = parametersKey.GetValue( "AppParameters" ) as string;
                        workingDirectory = parametersKey.GetValue( "AppDirectory" ) as string;
                    }
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, $"Failed to read registry parameters for service {_serviceName}" );
                    throw;
                }

                if( string.IsNullOrEmpty( executablePath ) )
                {
                    _logger.LogError( $"No executable path found in registry for service {_serviceName}" );
                    throw new InvalidOperationException( "No executable path specified" );
                }

                _logger.LogInformation( $"Starting application: {executablePath} {arguments}" );

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName( executablePath ) ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _managedProcess = new System.Diagnostics.Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                //set up event handlers and capture output
                _managedProcess.Exited += OnProcessExited;
                _managedProcess.OutputDataReceived += ( sender, e ) =>
                {
                    if( !string.IsNullOrEmpty( e.Data ) )
                        _logger.LogInformation( $"[{_serviceName}] {e.Data}" );
                };

                _managedProcess.ErrorDataReceived += ( sender, e ) =>
                {
                    if( !string.IsNullOrEmpty( e.Data ) )
                        _logger.LogError( $"[{_serviceName}] {e.Data}" );
                };

                if( !_managedProcess.Start() )
                {
                    _logger.LogError( $"Failed to start process: {executablePath}" );
                    throw new InvalidOperationException( $"Failed to start process: {executablePath}" );
                }

                _managedProcess.BeginOutputReadLine();
                _managedProcess.BeginErrorReadLine();

                _logger.LogInformation( $"Successfully started application with PID {_managedProcess.Id}" );

                //start a thread to monitor the process
                Task.Run( () => MonitorProcessAsync( _cts.Token ) );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, $"Error starting service {_serviceName}" );
                throw;
            }
        }

        protected override void OnStop()
        {
            _logger.LogInformation( $"Stopping service {_serviceName}" );

            _cts?.Cancel();

            // Try to gracefully stop the process
            if( _managedProcess != null && !_managedProcess.HasExited )
            {
                try
                {
                    //first try to close the process gracefully
                    _logger.LogInformation( $"Attempting to gracefully close process with PID {_managedProcess.Id}" );
                    _managedProcess.CloseMainWindow();

                    //wait up to 5 seconds for the process to exit
                    if( !_managedProcess.WaitForExit( 5000 ) )
                    {
                        //if it doesn't exit, force kill it
                        _logger.LogWarning( $"Process did not exit gracefully, killing process with PID {_managedProcess.Id}" );
                        _managedProcess.Kill();
                    }
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, $"Error stopping process for service {_serviceName}" );
                }
            }

            if( _managedProcess != null )
            {
                _managedProcess.Exited -= OnProcessExited;
                _managedProcess.Dispose();
                _managedProcess = null;
            }

            _logger.LogInformation( $"Service {_serviceName} stopped successfully" );
        }

        private void OnProcessExited(
            object? sender,
            EventArgs e
        )
        {
            if( _managedProcess == null ) return;

            var exitCode = _managedProcess.ExitCode;
            _logger.LogWarning( $"Process for service {_serviceName} exited with code {exitCode}" );

            //check if we should restart the process based on exit code
            var shouldRestart = ShouldRestartOnExit( exitCode );

            if( shouldRestart && _cts != null && !_cts.IsCancellationRequested )
            {
                _logger.LogInformation( $"Restarting process for service {_serviceName}" );
                try
                {
                    OnStart( Array.Empty<string>() );
                }
                catch( Exception ex )
                {
                    _logger.LogError( ex, $"Failed to restart process for service {_serviceName}" );
                }
            }
        }

        private async Task MonitorProcessAsync( CancellationToken cancellationToken )
        {
            try
            {
                while( !cancellationToken.IsCancellationRequested )
                {
                    await Task.Delay( 5000, cancellationToken );
                    if( _managedProcess != null && _managedProcess.HasExited )
                    {
                        _logger.LogWarning( $"Process for service {_serviceName} has exited unexpectedly" );
                        break;
                    }
                }
            }
            catch( OperationCanceledException )
            {
                //normal cancellation
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, $"Error monitoring process for service {_serviceName}" );
            }
        }

        private bool ShouldRestartOnExit( int exitCode )
        {
            //restart on all non-zero exit codes
            try
            {
                using var parametersKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    $"SYSTEM\\CurrentControlSet\\Services\\{_serviceName}\\Parameters" );
                if( parametersKey != null )
                {
                    string exitActionKey = $"AppExit\\{exitCode}";
                    string? exitAction = parametersKey.GetValue( exitActionKey ) as string;

                    //if no specific exit code action, check default
                    if( string.IsNullOrEmpty( exitAction ) )
                    {
                        exitAction = parametersKey.GetValue( "AppExit" ) as string;
                    }

                    //if no action specified, default to restart
                    if( string.IsNullOrEmpty( exitAction ) )
                    {
                        return true;
                    }

                    return string.Equals( exitAction, "Restart", StringComparison.OrdinalIgnoreCase );
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, $"Error determining restart policy for service {_serviceName}" );
            }

            //default to restart
            return true;
        }
    }
}