using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSSM.CLI.Commands;
using NSSM.CLI.Constants;
using NSSM.CLI.DependencyInjection;
using NSSM.CLI.Service;
using NSSM.Core.Process;
using NSSM.Core.Registry;
using NSSM.Core.Service;

namespace NSSM.CLI
{
    /// <summary>
    /// Entry point for the NSSM CLI application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        public static async Task<int> Main( string[] args )
        {
            try
            {
                if( args.Length > 0 && args[0] == CliConstants.RunAsServiceArg )
                {
                    Console.WriteLine( "Starting NSSM in service mode..." );

                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddLogging( builder =>
                    {
                        builder.AddConsole();
                        builder.AddEventLog( options => options.SourceName = "NSSM Service Host" );
                    } );

                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    var serviceModeHandler = new ServiceModeHandler( serviceProvider );
                    return serviceModeHandler.Run( args );
                }

                using var host = CreateHostBuilder( args ).Build();
                var rootCommand = new RootCommand( "Non-Sucking Service Manager - Manages Windows services" );
                CommandRegistration.RegisterCommands( rootCommand, host.Services );
                var result = await rootCommand.InvokeAsync( args );

                if( !Console.IsInputRedirected && !Console.IsOutputRedirected )
                {
                    Console.WriteLine( "\nPress any key to exit..." );
                    Console.ReadKey( true );
                }

                return result;
            }
            catch( System.ComponentModel.Win32Exception ex ) when( ex.NativeErrorCode == 740 )
            {
                Console.Error.WriteLine( "Error: This application requires administrative privileges." );
                Console.Error.WriteLine( "Please run this application as Administrator." );
                return 740;
            }
            catch( Exception ex )
            {
                Console.Error.WriteLine( $"Error: {ex.Message}" );
                return 1;
            }
        }

        /// <summary>
        /// Creates and configures the host builder with dependency injection
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Configured host builder</returns>
        private static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureServices( ( hostContext, services ) =>
                {
                    services.AddLogging( configure => configure.AddConsole() );
                    services.AddSingleton<ServiceManager>();
                    services.AddSingleton<RegistryManager>();
                    services.AddSingleton<ProcessManager>();
                    services.AddCliServices();
                } );
    }
}