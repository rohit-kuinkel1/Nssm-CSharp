# NSSM-C# - Non-Sucking Service Manager for Windows

NSSM-C# is a modern C# implementation of the popular NSSM (Non-Sucking Service Manager) tool for Windows. It provides a powerful and intuitive way to manage Windows services through both a command-line interface and a graphical user interface.

## Features

- **Create and manage Windows services** with ease
- **Simple installation/removal** of services
- **Start, stop, pause, and restart** services
- **Configure service properties** like startup type, credentials, dependencies, etc.
- **Recover from crashes** automatically with configurable recovery options
- **Log service output** to files or the console
- **Modern Material Design UI** for easy management
- **Powerful CLI** for scriptable service management
- **Administrative features** to manage services remotely

## Installation

### Prerequisites

- Windows 10/11 or Windows Server 2016/2019/2022
- .NET 8.0 Runtime or SDK
- Administrator privileges (required for service management)

### Download Options

1. **Download Latest Build**
   - Download the latest version of either the CLI tool or WPF application from the repository

2. **Build from Source**
   ```powershell
   git clone https://github.com/username/NSSM-CSharp.git
   cd NSSM-C#
   dotnet build -c Release
   ```

## Using the Command-Line Interface (CLI)

NSSM-C# includes a powerful command-line interface for managing services programmatically or through scripts.

> **Note:** You must run the CLI with administrative privileges to manage services.

### Basic Commands

#### Install a Service

```powershell
nssm install <service-name> <executable-path> [options]
```

**Options:**
- `--display-name <display-name>` - Set a friendly display name
- `--description <description>` - Set a description
- `--arguments <arguments>` - Command line arguments
- `--startup <startup-type>` - Service startup type (AutoStart, Manual, Disabled)
- `--username <username>` - Run service as this user
- `--password <password>` - Password for username

**Example:**
```powershell
nssm install MyService "C:\MyApp\service.exe" --display-name "My Custom Service" --description "This is my custom service" --startup AutoStart
```

#### Remove a Service

```powershell
nssm remove <service-name>
```

### Running the CLI

Use the provided batch file for convenience:

```powershell
run-as-admin.bat install MyService "C:\path\to\app.exe"
```

## Using the Graphical User Interface (GUI)

The NSSM-C# GUI provides an intuitive interface for managing all your Windows services.

### Starting the GUI

1. Run `NSSM.WPF.exe` with administrative privileges, or
2. Use the provided batch file: `run-nssm-wpf.bat`

### Main Interface

The GUI is divided into three main sections:

1. **Service List**: Shows all installed Windows services with their status
2. **Service Details**: Displays and allows editing of the selected service's configuration
3. **Console Output**: Shows log output and operation results

### Managing Services

- **Installing a Service**: Click the "Install Service" button and fill in the required information
- **Starting/Stopping Services**: Select a service and use the corresponding buttons
- **Modifying Service Properties**: Select a service and edit properties in the details panel
- **Removing a Service**: Select a service and click the "Remove" button

## Best Practices

1. **Always backup** your service configurations before making changes
2. **Use descriptive names** for your services
3. **Set appropriate recovery options** for critical services
4. **Configure proper logging** to diagnose issues
5. **Test service installations** in development environments first

## Troubleshooting

### Common Issues and Solutions

- **Access Denied Errors**: Make sure you're running with administrator privileges
- **Service Won't Start**: Check the application dependencies and event logs
- **Service Fails to Install**: Ensure the path to the executable is correct

### Logging

Enable detailed logging by editing the `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Contributing

Contributions to NSSM-C# are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Original NSSM project for inspiration
- All contributors to this project
