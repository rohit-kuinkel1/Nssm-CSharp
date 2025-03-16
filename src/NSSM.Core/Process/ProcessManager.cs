using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NSSM.Core.Service.Enums;

namespace NSSM.Core.Process
{
    /// <summary>
    /// Handles process management for NSSM services
    /// This is a C# implementation of the functionality in process.cpp
    /// </summary>
    public class ProcessManager
    {
        private readonly ILogger<ProcessManager> _logger;

        //constants from the original C++ code
        public const int DefaultKillProcessDelay = 1500;
        public const int DefaultKillConsoleDelay = 1500;
        public const int DefaultKillWindowDelay = 1500;
        public const int DefaultKillThreadsDelay = 1000;

        public ProcessManager( ILogger<ProcessManager> logger )
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts a process with the specified parameters
        /// </summary>
        /// <param name="executablePath">Path to the executable</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="environmentVariables">Environment variables</param>
        /// <param name="priority">Process priority</param>
        /// <param name="noWindow">Whether to hide the window</param>
        /// <returns>Process object if successful, null otherwise</returns>
        public System.Diagnostics.Process? StartProcess(
            string executablePath,
            string arguments,
            string workingDirectory,
            IDictionary<string, string>? environmentVariables = null,
            ProcessPriority priority = ProcessPriority.Normal,
            bool noWindow = false
        )
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = noWindow
                };

                //add environment vars if provided
                if( environmentVariables != null )
                {
                    foreach( var variable in environmentVariables )
                    {
                        startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                    }
                }

                var process = System.Diagnostics.Process.Start( startInfo );
                if( process == null )
                {
                    _logger.LogError( "Failed to start process {ExecutablePath}", executablePath );
                    return null;
                }

                SetProcessPriority( process, priority );

                return process;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error starting process {ExecutablePath}", executablePath );
                return null;
            }
        }

        /// <summary>
        /// Sets the priority of a process
        /// </summary>
        /// <param name="process">Process to set priority for</param>
        /// <param name="priority">Priority to set</param>
        private void SetProcessPriority(
            System.Diagnostics.Process process,
            ProcessPriority priority
        )
        {
            try
            {
                switch( priority )
                {
                    case ProcessPriority.Realtime:
                        process.PriorityClass = ProcessPriorityClass.RealTime;
                        break;
                    case ProcessPriority.High:
                        process.PriorityClass = ProcessPriorityClass.High;
                        break;
                    case ProcessPriority.AboveNormal:
                        process.PriorityClass = ProcessPriorityClass.AboveNormal;
                        break;
                    case ProcessPriority.Normal:
                        process.PriorityClass = ProcessPriorityClass.Normal;
                        break;
                    case ProcessPriority.BelowNormal:
                        process.PriorityClass = ProcessPriorityClass.BelowNormal;
                        break;
                    case ProcessPriority.Idle:
                        process.PriorityClass = ProcessPriorityClass.Idle;
                        break;
                }
            }
            catch( Exception ex )
            {
                _logger.LogWarning( ex, "Failed to set process priority" );
            }
        }

        /// <summary>
        /// Terminates a process gracefully, then forcefully if necessary
        /// </summary>
        /// <param name="process">Process to terminate</param>
        /// <param name="killTree">Whether to kill the process tree</param>
        /// <param name="consoleDelay">Delay before killing console window</param>
        /// <param name="windowDelay">Delay before killing window</param>
        /// <param name="threadsDelay">Delay before killing threads</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool TerminateProcess(
            System.Diagnostics.Process process,
            bool killTree = true,
            int consoleDelay = DefaultKillConsoleDelay,
            int windowDelay = DefaultKillWindowDelay,
            int threadsDelay = DefaultKillThreadsDelay
        )
        {
            try
            {
                //try to close gracefully first
                if( !process.HasExited )
                {
                    if( process.CloseMainWindow() )
                    {
                        if( process.WaitForExit( windowDelay ) )
                            return true;
                    }

                    if( SendCtrlC( process.Id ) )
                    {
                        if( process.WaitForExit( consoleDelay ) )
                            return true;
                    }

                    if( killTree )
                    {
                        KillProcessTree( process.Id, threadsDelay );
                    }
                    else
                    {
                        process.Kill();
                    }

                    return process.WaitForExit( threadsDelay );
                }

                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error terminating process {ProcessId}", process.Id );
                return false;
            }
        }

        /// <summary>
        /// Kills a process and all its child processes
        /// </summary>
        /// <param name="processId">ID of the process to kill</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool KillProcessTree(
            int processId,
            int timeout = DefaultKillThreadsDelay
        )
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcesses();
                var children = new List<System.Diagnostics.Process>();

                foreach( var proc in processes )
                {
                    try
                    {
                        if( IsChildProcess( proc, processId ) )
                            children.Add( proc );
                    }
                    catch
                    {
                        //ignore errors when checking processes
                    }
                }

                //kill child processes first
                foreach( var child in children )
                {
                    try
                    {
                        if( !child.HasExited )
                            child.Kill();
                    }
                    catch
                    {
                        //ignore errors when killing processes
                    }
                }

                //kill the main process at last so as to not leave zombies
                var process = System.Diagnostics.Process.GetProcessById( processId );
                if( !process.HasExited )
                {
                    process.Kill();
                }

                return process.WaitForExit( timeout );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, "Error killing process tree {ProcessId}", processId );
                return false;
            }
        }

        /// <summary>
        /// Determines if a process is a child of another process
        /// </summary>
        /// <param name="process">Process to check</param>
        /// <param name="parentId">ID of the parent process</param>
        /// <returns>True if the process is a child, false otherwise</returns>
        private bool IsChildProcess(
            System.Diagnostics.Process process,
            int parentId
        )
        {
            // This is a simplified implementation. In a real implementation, you would use
            // the Windows Management Instrumentation (WMI) or native Windows API calls to
            // determine the parent-child relationship between processes.
            try
            {
                var parentProcess = ProcessNativeMethods.GetParentProcess( process.Id );
                return parentProcess?.Id == parentId;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a Ctrl+C signal to a process
        /// </summary>
        /// <param name="processId">ID of the process</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool SendCtrlC( int processId )
        {
            try
            {
                return ProcessNativeMethods.AttachConsole( (uint)processId ) &&
                       ProcessNativeMethods.SetConsoleCtrlHandler( null, true ) &&
                       ProcessNativeMethods.GenerateConsoleCtrlEvent( ProcessNativeMethods.CTRL_C_EVENT, 0 ) &&
                       ProcessNativeMethods.FreeConsole() &&
                       ProcessNativeMethods.SetConsoleCtrlHandler( null, false );
            }
            catch( Exception ex )
            {
                _logger.LogWarning( ex, "Failed to send Ctrl+C to process {ProcessId}", processId );
                return false;
            }
        }
    }
}