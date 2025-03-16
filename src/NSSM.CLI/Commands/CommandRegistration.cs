using System.CommandLine;
using NSSM.Core.Service;
using NSSM.Core.Service.Enums;

namespace NSSM.CLI.Commands
{
    /// <summary>
    /// Configures and registers all command-line commands, arguments, and options for the CLI
    /// </summary>
    public static class CommandRegistration
    {
        /// <summary>
        /// Registers all commands for the CLI application
        /// </summary>
        /// <param name="rootCommand">Root command for the application</param>
        /// <param name="services">Service provider for DI</param>
        public static void RegisterCommands(
            RootCommand rootCommand,
            IServiceProvider services
        )
        {
            RegisterInstallCommand( rootCommand, services );
            RegisterRemoveCommand( rootCommand, services );
        }

        /// <summary>
        /// Registers the install command to create new Windows services
        /// <paramref name="rootCommand"/>Root command for the application</param>
        /// <param name="services">Service provider for DI</param>
        /// </summary>
        private static void RegisterInstallCommand(
            RootCommand rootCommand,
            IServiceProvider services
        )
        {
            var installCommand = new Command( "install", "Install a new service" );

            var serviceNameArg = new Argument<string>( "service-name", "Name of the service" );
            var executablePathArg = new Argument<string>( "executable-path", "Path to the executable" );

            var displayNameOption = new Option<string>( "--display-name", "Display name of the service" );
            var descriptionOption = new Option<string>( "--description", "Description of the service" );
            var argumentsOption = new Option<string>( "--arguments", "Command line arguments" );
            var startupOption = new Option<ServiceStartup>( "--startup", () => ServiceStartup.AutoStart, "Service startup type" );
            var usernameOption = new Option<string>( "--username", "Username to run the service as" );
            var passwordOption = new Option<string>( "--password", "Password for the username" );

            installCommand.AddArgument( serviceNameArg );
            installCommand.AddArgument( executablePathArg );
            installCommand.AddOption( displayNameOption );
            installCommand.AddOption( descriptionOption );
            installCommand.AddOption( argumentsOption );
            installCommand.AddOption( startupOption );
            installCommand.AddOption( usernameOption );
            installCommand.AddOption( passwordOption );

            var installHandler = new InstallCommandHandler( services );

            installCommand.SetHandler<
                string, string, string, string, string, ServiceStartup, string, string>
                (
                    ( serviceName, executablePath, displayName, description, arguments, startup, username, password ) =>
                        installHandler.HandleAsync( serviceName, executablePath, displayName,
                            description,
                            arguments,
                            startup,
                            username,
                            password
                        ),
                    serviceNameArg,
                    executablePathArg,
                    displayNameOption,
                    descriptionOption,
                    argumentsOption,
                    startupOption,
                    usernameOption,
                    passwordOption
                );

            rootCommand.AddCommand( installCommand );
        }

        /// <summary>
        /// Registers the remove command to uninstall Windows services
        /// <paramref name="rootCommand"/>Root command for the application</param>
        /// <param name="services">Service provider for DI</param>
        /// </summary>
        private static void RegisterRemoveCommand(
            RootCommand rootCommand,
            IServiceProvider services
        )
        {
            var removeCommand = new Command( "remove", "Remove an existing service" );
            var removeServiceNameArg = new Argument<string>( "service-name", "Name of the service to remove" );

            removeCommand.AddArgument( removeServiceNameArg );

            var removeHandler = new RemoveCommandHandler( services );

            removeCommand.SetHandler<string>(
                ( serviceName ) => removeHandler.HandleAsync( serviceName ),
                removeServiceNameArg );

            rootCommand.AddCommand( removeCommand );
        }
    }
}