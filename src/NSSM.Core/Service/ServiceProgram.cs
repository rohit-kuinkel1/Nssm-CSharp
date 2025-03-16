using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace NSSM.Core.Service
{
    /// <summary>
    /// Service entry point that runs when Windows starts the service
    /// </summary>
    public static class ServiceProgram
    {
        private static NssmServiceHost? _serviceHost;
        private static bool _isRunningAsService = false;
        private static ManualResetEvent _serviceStoppedEvent = new ManualResetEvent(false);
        
        /// <summary>
        /// Checks if the current process is running as a service
        /// </summary>
        /// <returns>True if running as a service, false otherwise</returns>
        public static bool IsRunningAsService()
        {
            return _isRunningAsService;
        }
        
        /// <summary>
        /// Entry point when running as a service
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void RunAsService(string[] args)
        {
            _isRunningAsService = true;
            
            //if we're running from the command line but meant to be a service,
            //we can detect that here and run the service directly
            if (!Environment.UserInteractive)
            {
                //normal service startup
                ServiceBase[] servicesToRun = GetServicesToRun(args);
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                //running from console as a service, useful for debugging
                Console.WriteLine("Starting service in console mode...");
                
                ServiceBase[] servicesToRun = GetServicesToRun(args);
                foreach (var service in servicesToRun)
                {
                    Task.Run(() => 
                    {
                        typeof(ServiceBase).GetMethod("OnStart", System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance)?.Invoke(service, new object[] { args });
                        
                        Console.WriteLine($"Service {service.ServiceName} started.");
                    });
                }
                
                Console.WriteLine("Press any key to stop the service...");
                Console.ReadKey();
                
                foreach (var service in servicesToRun)
                {
                    typeof(ServiceBase).GetMethod("OnStop", System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance)?.Invoke(service, null);
                        
                    Console.WriteLine($"Service {service.ServiceName} stopped.");
                }
            }
        }
        
        /// <summary>
        /// Gets the services to run
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Array of services to run</returns>
        private static ServiceBase[] GetServicesToRun(string[] args)
        {
            var services = new ServiceCollection();   
            services.AddLogging(builder => 
            {
                builder.AddEventLog(options => 
                {
                    options.SourceName = "NSSM Service Host";
                });
                
                //we can add file logging too but I will let it be for now
                //builder.AddFile("logs/nssm-service-{Date}.log");
            });
            
            var serviceProvider = services.BuildServiceProvider();        
            var logger = serviceProvider.GetRequiredService<ILogger<NssmServiceHost>>();
            
            // determine service name - either from args or assume it's the process name
            var serviceName = "NSSM";
            if (args.Length > 0)
            {
                serviceName = args[0];
            }
            else
            {
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (!string.IsNullOrEmpty(processName) && !processName.Equals("NSSM", StringComparison.OrdinalIgnoreCase))
                {
                    serviceName = processName;
                }
            }
            
            logger.LogInformation($"Starting service host for {serviceName}");         
            var serviceHost = new NssmServiceHost(serviceName, logger);
            
            return new ServiceBase[] { serviceHost };
        }
        
        /// <summary>
        /// Waits for the service to be stopped
        /// </summary>
        public static void WaitForServiceStopped()
        {
            _serviceStoppedEvent.WaitOne();
        }
        
        /// <summary>
        /// Signals that the service has been stopped
        /// </summary>
        public static void SignalServiceStopped()
        {
            _serviceStoppedEvent.Set();
        }
    }
} 