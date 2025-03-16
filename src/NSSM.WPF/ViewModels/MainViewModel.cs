using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NSSM.Core.Constants.AppConstants;
using NSSM.Core.Logging;
using NSSM.Core.Messaging;
using NSSM.Core.Services.Interfaces;
using NSSM.WPF.Models;
using NSSM.WPF.Views;

namespace NSSM.WPF.ViewModels
{

    /// <summary>
    /// Main view model for the application's main window.
    /// Handles service management operations and UI coordination.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IWindowsServiceManager _serviceManager;
        private readonly LoggingService _loggingService;
        private readonly IMessageBus _messageBus;

        private ObservableCollection<ServiceModel> _allServices = new();
        private ICollectionView _filteredServices;
        private ServiceModel? _selectedService;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isAdmin;
        private string _searchText = string.Empty;
        private string _consoleOutput = string.Empty;
        private string _consoleSearchText = string.Empty;
        private readonly object _consoleLock = new object();
        private int _maxConsoleLines = 1000; //maximum number of lines to keep in the console
        private string _selectedLogLevel = "INFO"; //default to showing INFO logs
        private List<string> _rawLogLines = new List<string>(); //store all log lines for filtering

        /// <summary>
        /// Available log levels for filtering
        /// </summary>
        public List<string> LogLevels { get; } = new List<string> { "All", "INFO", "DEBUG", "WARNING", "ERROR" };

        /// <summary>
        /// Maximum number of log lines to retain in the console
        /// </summary>
        public int MaxConsoleLines
        {
            get => _maxConsoleLines;
            set
            {
                //ensure the value is at least 100 lines to prevent too small values
                var newValue = Math.Max( MaxConsoleLines, value );
                if( SetProperty( ref _maxConsoleLines, newValue ) )
                {
                    _loggingService.LogInformation( $"Maximum console lines changed to {newValue}" );

                    //if we already have more lines than the new maximum, trim them
                    lock( _consoleLock )
                    {
                        if( _rawLogLines.Count > _maxConsoleLines )
                        {
                            _rawLogLines = _rawLogLines.Skip( _rawLogLines.Count - _maxConsoleLines ).ToList();
                            UpdateFilteredConsoleOutput();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The currently selected log level filter
        /// </summary>
        public string SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                if( SetProperty( ref _selectedLogLevel, value ) )
                {
                    //apply both filters
                    if( string.IsNullOrWhiteSpace( _consoleSearchText ) )
                    {
                        UpdateConsoleOutputWithLogLevelFilter();
                    }
                    else
                    {
                        ApplyConsoleFilter();
                    }

                    _loggingService.LogInformation( $"Console log level filter set to: {value}" );
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="serviceManager">Windows service manager interface</param>
        /// <param name="loggingService">Logging service</param>
        /// <param name="messageBus">Message bus for event communication</param>
        public MainViewModel(
            IWindowsServiceManager serviceManager,
            LoggingService loggingService,
            IMessageBus messageBus )
        {
            _serviceManager = serviceManager ?? throw new ArgumentNullException( nameof( serviceManager ) );
            _loggingService = loggingService ?? throw new ArgumentNullException( nameof( loggingService ) );
            _messageBus = messageBus ?? throw new ArgumentNullException( nameof( messageBus ) );

            _isAdmin = CheckAdministratorAccess();
            _loggingService.LogInformation( $"Application running with administrator privileges: {_isAdmin}" );

            //setup filtered collection view
            _filteredServices = CollectionViewSource.GetDefaultView( _allServices );
            _filteredServices.SortDescriptions.Add( new SortDescription( "DisplayName", ListSortDirection.Ascending ) );
            _filteredServices.Filter = FilterServices;

            //initialize commands
            RefreshServicesCommand = new RelayCommand( _ => RefreshServices() );
            StartServiceCommand = new RelayCommand( _ => StartSelectedService(), _ => CanModifyService() );
            StopServiceCommand = new RelayCommand( _ => StopSelectedService(), _ => CanModifyService() );
            RestartServiceCommand = new RelayCommand( _ => RestartSelectedService(), _ => CanModifyService() );
            RemoveServiceCommand = new RelayCommand( _ => RemoveSelectedService(), _ => CanModifyService() );
            InstallServiceCommand = new RelayCommand( _ => ShowInstallServiceDialog() );
            RunAsAdminCommand = new RelayCommand( _ => HandleAdminRequest(), _ => true );
            FocusSearchCommand = new RelayCommand( _ => FocusSearchBox() );
            ClearConsoleCommand = new RelayCommand( _ => ClearConsole() );
            ResetMaxConsoleLinesCommand = new RelayCommand( _ => ResetMaxConsoleLines() );

            //subscribe to messages
            _messageBus.Subscribe<ServiceInstalledMessage>( OnServiceInstalled );
            _messageBus.Subscribe<ServiceRemovedMessage>( OnServiceRemoved );
            _messageBus.Subscribe<ServiceChangedMessage>( OnServiceChanged );

            //subscribe to log messages
            _loggingService.LogMessageReceived += OnLogMessageReceived;

            AppendToConsole( $"NSSM Console Started - {DateTime.Now}" );
            AppendToConsole( "----------------------------------------------------" );

            RefreshServices();
        }

        public ObservableCollection<ServiceModel> AllServices
        {
            get => _allServices;
            set => SetProperty( ref _allServices, value );
        }

        public ServiceModel? SelectedService
        {
            get => _selectedService;
            set
            {
                if( SetProperty( ref _selectedService, value ) && value != null )
                {
                    _loggingService.LogDebug(
                        $"Service selected: Name='{value.DisplayName}', " +
                        $"Status='{value.Status}', " +
                        $"StartupType='{value.StartupType}', " +
                        $"ServiceName='{value.ServiceName}'"
                    );
                    _loggingService.LogDebug(
                        $"Name='{value.DisplayName}', Executable='{value.ExecutablePath}', " +
                        $"Arguments='{value.Arguments}', " +
                        $"Account='{value.Account}'"
                    );
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty( ref _isLoading, value );
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty( ref _statusMessage, value );
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty( ref _isAdmin, value );
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if( SetProperty( ref _searchText, value ) )
                {
                    _filteredServices.Refresh();
                    _loggingService.LogDebug( $"Filtering services with search term: {value}" );
                }
            }
        }

        public string ConsoleSearchText
        {
            get => _consoleSearchText;
            set
            {
                if( SetProperty( ref _consoleSearchText, value ) )
                {
                    ApplyConsoleFilter();
                }
            }
        }

        public ICollectionView Services
        {
            get => _filteredServices;
        }

        public ICommand RefreshServicesCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }
        public ICommand RestartServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public ICommand InstallServiceCommand { get; }
        public ICommand RunAsAdminCommand { get; }
        public ICommand FocusSearchCommand { get; }
        public ICommand ClearConsoleCommand { get; }
        public ICommand ResetMaxConsoleLinesCommand { get; }

        public string ConsoleOutput
        {
            get => _consoleOutput;
            private set => SetProperty( ref _consoleOutput, value );
        }

        private bool CanModifyService() => SelectedService != null;

        private bool CheckAdministratorAccess()
        {
            _loggingService.LogDebug( "Checking administrator access..." );
            try
            {
                //try to access a protected service operation
                try
                {
                    var testService = _serviceManager.GetServiceInfoAsync( "W32Time" ).GetAwaiter().GetResult();
                    _loggingService.LogDebug( "Administrator access check through service API successful" );
                }
                catch( UnauthorizedAccessException )
                {
                    _loggingService.LogDebug( "Service API check failed: UnauthorizedAccessException" );
                    return false;
                }

                //then check Windows identity directly (belt and suspenders approach)
                var isElevated = false;
                using( var identity = System.Security.Principal.WindowsIdentity.GetCurrent() )
                {
                    var principal = new System.Security.Principal.WindowsPrincipal( identity );
                    isElevated = principal.IsInRole( System.Security.Principal.WindowsBuiltInRole.Administrator );
                    _loggingService.LogDebug( $"Windows identity check for administrator: {isElevated}" );
                }

                bool result = isElevated;
                _loggingService.LogInformation( $"Application running with administrator privileges: {result}" );
                return result;
            }
            catch( Exception ex )
            {
                _loggingService.LogWarning( $"Error checking administrator access: {ex.Message}", ex );
                //if we get a different exception, assume we don't have admin rights
                return false;
            }
        }

        private async void RefreshServices()
        {
            _loggingService.LogDebug( "Beginning service refresh operation" );
            IsLoading = true;
            StatusMessage = "Loading services...";
            _loggingService.LogInformation( "Refreshing services list" );

            try
            {
                _loggingService.LogDebug( "Calling service manager to get all services" );
                var services = await _serviceManager.GetAllServicesAsync();
                _loggingService.LogDebug( $"Retrieved {services.Count()} services from service manager" );

                System.Windows.Application.Current.Dispatcher.Invoke( () =>
                {
                    _loggingService.LogDebug( "Processing service list on UI thread" );
                    _allServices.Clear();
                    foreach( var service in services )
                    {
                        _allServices.Add( new ServiceModel
                        {
                            ServiceName = service.Name,
                            DisplayName = service.DisplayName,
                            Status = service.Status,
                            StartupType = service.StartType,
                            Description = service.Description,
                            ExecutablePath = service.BinaryPath,
                            //Arguments and WorkingDirectory might need custom parsing from BinaryPath
                            Account = service.Account
                        } );
                    }

                    // Refresh filtered view
                    _filteredServices.Refresh();

                    StatusMessage = $"Loaded {services.Count()} services";
                    _loggingService.LogInformation( $"Loaded {services.Count()} services" );
                    _loggingService.LogDebug( "Service refresh operation completed successfully" );
                } );
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"Service refresh failed with exception: {ex.GetType().Name}" );
                StatusMessage = $"Error loading services: {ex.Message}";
                _loggingService.LogError( $"Error loading services: {ex.Message}", ex );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void StartSelectedService()
        {
            if( SelectedService == null )
            {
                _loggingService.LogDebug( "StartSelectedService called with no service selected" );
                return;
            }

            _loggingService.LogDebug( $"Beginning service start operation for '{SelectedService.ServiceName}'" );
            IsLoading = true;
            var serviceName = SelectedService.ServiceName;
            var displayName = SelectedService.DisplayName;
            StatusMessage = $"Starting service '{displayName}'...";
            _loggingService.LogInformation( $"Starting service '{serviceName}'" );

            try
            {
                _loggingService.LogDebug( $"Calling service manager to start service '{serviceName}'" );
                var success = await _serviceManager.StartServiceAsync( serviceName );

                if( success )
                {
                    StatusMessage = $"Service '{displayName}' started successfully";
                    _loggingService.LogInformation( $"Service '{serviceName}' started successfully" );
                    _loggingService.LogDebug( $"Service start operation successful for '{serviceName}'" );

                    //notify other components about the service change
                    _loggingService.LogDebug( $"Publishing ServiceChangedMessage for '{serviceName}'" );
                    _messageBus.Publish( new ServiceChangedMessage( serviceName ) );

                    //refresh the selected service
                    await RefreshSelectedServiceAsync();
                }
                else
                {
                    _loggingService.LogDebug( $"Service start operation failed for '{serviceName}'" );
                    StatusMessage = $"Failed to start service '{displayName}'";
                    _loggingService.LogWarning( $"Failed to start service '{serviceName}'" );

                    if( !_isAdmin )
                    {
                        _loggingService.LogDebug( "Admin privileges required for starting service - showing admin prompt" );
                        ShowAdminRequiredMessage( "start services" );
                    }
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"Service start operation threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Error starting service: {ex.Message}";
                _loggingService.LogError( $"Error starting service '{serviceName}': {ex.Message}", ex );
            }
            finally
            {
                IsLoading = false;
                _loggingService.LogDebug( $"Service start operation completed for '{serviceName}'" );
            }
        }

        private async void StopSelectedService()
        {
            if( SelectedService == null )
            {
                _loggingService.LogDebug( "StopSelectedService called with no service selected" );
                return;
            }

            _loggingService.LogDebug( $"Beginning service stop operation for '{SelectedService.ServiceName}'" );
            IsLoading = true;
            var serviceName = SelectedService.ServiceName;
            var displayName = SelectedService.DisplayName;
            StatusMessage = $"Stopping service '{displayName}'...";
            _loggingService.LogInformation( $"Stopping service '{serviceName}'" );

            try
            {
                _loggingService.LogDebug( $"Calling service manager to stop service '{serviceName}'" );
                var success = await _serviceManager.StopServiceAsync( serviceName );

                if( success )
                {
                    StatusMessage = $"Service '{displayName}' stopped successfully";
                    _loggingService.LogInformation( $"Service '{serviceName}' stopped successfully" );
                    _loggingService.LogDebug( $"Service stop operation successful for '{serviceName}'" );

                    //notify other components about the service change
                    _loggingService.LogDebug( $"Publishing ServiceChangedMessage for '{serviceName}'" );
                    _messageBus.Publish( new ServiceChangedMessage( serviceName ) );

                    //refresh the selected service
                    await RefreshSelectedServiceAsync();
                }
                else
                {
                    _loggingService.LogDebug( $"Service stop operation failed for '{serviceName}'" );
                    StatusMessage = $"Failed to stop service '{displayName}'";
                    _loggingService.LogWarning( $"Failed to stop service '{serviceName}'" );

                    if( !_isAdmin )
                    {
                        _loggingService.LogDebug( "Admin privileges required for stopping service - showing admin prompt" );
                        ShowAdminRequiredMessage( "stop services" );
                    }
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"Service stop operation threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Error stopping service: {ex.Message}";
                _loggingService.LogError( $"Error stopping service '{serviceName}': {ex.Message}", ex );
            }
            finally
            {
                IsLoading = false;
                _loggingService.LogDebug( $"Service stop operation completed for '{serviceName}'" );
            }
        }

        private async void RestartSelectedService()
        {
            if( SelectedService == null )
            {
                _loggingService.LogDebug( "RestartSelectedService called with no service selected" );
                return;
            }

            _loggingService.LogDebug( $"Beginning service restart operation for '{SelectedService.ServiceName}'" );
            IsLoading = true;
            var serviceName = SelectedService.ServiceName;
            var displayName = SelectedService.DisplayName;
            StatusMessage = $"Restarting service '{displayName}'...";
            _loggingService.LogInformation( $"Restarting service '{serviceName}'" );

            try
            {
                //stop the service first
                _loggingService.LogDebug( $"Attempting to stop service '{serviceName}' for restart" );
                var stopSuccess = await _serviceManager.StopServiceAsync( serviceName );
                if( !stopSuccess )
                {
                    _loggingService.LogDebug( $"Failed to stop service '{serviceName}' for restart" );
                    StatusMessage = $"Failed to stop service '{displayName}' for restart";
                    _loggingService.LogWarning( $"Failed to stop service '{serviceName}' for restart" );

                    if( !_isAdmin )
                    {
                        _loggingService.LogDebug( "Admin privileges required for restarting service - showing admin prompt" );
                        ShowAdminRequiredMessage( "restart services" );
                        IsLoading = false;
                        return;
                    }
                }
                else
                {
                    _loggingService.LogDebug( $"Successfully stopped service '{serviceName}' for restart" );
                }

                //then start it again
                _loggingService.LogDebug( $"Attempting to start service '{serviceName}' for restart" );
                var startSuccess = await _serviceManager.StartServiceAsync( serviceName );
                if( startSuccess )
                {
                    _loggingService.LogDebug( $"Successfully started service '{serviceName}' for restart" );
                    StatusMessage = $"Service '{displayName}' restarted successfully";
                    _loggingService.LogInformation( $"Service '{serviceName}' restarted successfully" );

                    //notify other components about the service change
                    _loggingService.LogDebug( $"Publishing ServiceChangedMessage for '{serviceName}'" );
                    _messageBus.Publish( new ServiceChangedMessage( serviceName ) );

                    await RefreshSelectedServiceAsync();
                }
                else
                {
                    _loggingService.LogDebug( $"Failed to start service '{serviceName}' for restart" );
                    StatusMessage = $"Failed to restart service '{displayName}'";
                    _loggingService.LogWarning( $"Failed to restart service '{serviceName}'" );

                    if( !_isAdmin )
                    {
                        _loggingService.LogDebug( "Admin privileges required for restarting service - showing admin prompt" );
                        ShowAdminRequiredMessage( "restart services" );
                    }
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"Service restart operation threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Error restarting service: {ex.Message}";
                _loggingService.LogError( $"Error restarting service '{serviceName}': {ex.Message}", ex );
            }
            finally
            {
                IsLoading = false;
                _loggingService.LogDebug( $"Service restart operation completed for '{serviceName}'" );
            }
        }

        private async void RemoveSelectedService()
        {
            if( SelectedService == null )
            {
                _loggingService.LogDebug( "RemoveSelectedService called with no service selected" );
                return;
            }

            var serviceName = SelectedService.ServiceName;
            var displayName = SelectedService.DisplayName;

            _loggingService.LogDebug( $"Showing confirmation dialog for removing service '{serviceName}'" );
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to remove service '{displayName}'?",
                UIConstants.WarningDialogTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if( result != MessageBoxResult.Yes )
            {
                _loggingService.LogDebug( $"User cancelled removal of service '{serviceName}'" );
                return;
            }

            _loggingService.LogDebug( $"Beginning service remove operation for '{serviceName}'" );
            IsLoading = true;
            StatusMessage = $"Removing service '{displayName}'...";
            _loggingService.LogInformation( $"Removing service '{serviceName}'" );

            try
            {
                _loggingService.LogDebug( $"Calling service manager to remove service '{serviceName}'" );
                var success = await _serviceManager.RemoveServiceAsync( serviceName );

                if( success )
                {
                    _loggingService.LogDebug( $"Service remove operation successful for '{serviceName}'" );
                    StatusMessage = $"Service '{displayName}' removed successfully";
                    _loggingService.LogInformation( $"Service '{serviceName}' removed successfully" );

                    //notify other components about the service removal
                    _loggingService.LogDebug( $"Publishing ServiceRemovedMessage for '{serviceName}'" );
                    _messageBus.Publish( new ServiceRemovedMessage( serviceName ) );

                    //remove from the collection
                    _loggingService.LogDebug( $"Removing service '{serviceName}' from UI collection" );
                    _allServices.Remove( SelectedService );
                    SelectedService = null;
                }
                else
                {
                    _loggingService.LogDebug( $"Service remove operation failed for '{serviceName}'" );
                    StatusMessage = $"Failed to remove service '{displayName}'";
                    _loggingService.LogWarning( $"Failed to remove service '{serviceName}'" );

                    if( !_isAdmin )
                    {
                        _loggingService.LogDebug( "Admin privileges required for removing service - showing admin prompt" );
                        ShowAdminRequiredMessage( "remove services" );
                    }
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"Service remove operation threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Error removing service: {ex.Message}";
                _loggingService.LogError( $"Error removing service '{serviceName}': {ex.Message}", ex );
            }
            finally
            {
                IsLoading = false;
                _loggingService.LogDebug( $"Service remove operation completed for '{serviceName}'" );
            }
        }

        private void ShowInstallServiceDialog()
        {
            _loggingService.LogDebug( "ShowInstallServiceDialog called" );

            if( !_isAdmin )
            {
                _loggingService.LogDebug( "Admin privileges required for installing services - showing admin prompt" );
                ShowAdminRequiredMessage( "install services" );
                return;
            }

            var mainWindow = System.Windows.Application.Current.MainWindow;
            if( mainWindow == null )
            {
                _loggingService.LogWarning( "Cannot show dialog - Main window is null" );
                return;
            }

            _loggingService.LogInformation( "Showing install service dialog" );

            //ensure we're on the UI thread
            if( !System.Windows.Application.Current.Dispatcher.CheckAccess() )
            {
                _loggingService.LogDebug( "Not on UI thread, invoking ShowInstallServiceDialog on UI thread" );
                System.Windows.Application.Current.Dispatcher.Invoke( () => ShowInstallServiceDialog() );
                return;
            }

            try
            {
                _loggingService.LogDebug( "Attempting to show InstallServiceDialog" );
                if( InstallServiceDialog.ShowDialog( mainWindow, out var result ) )
                {
                    if( result.Success )
                    {
                        _loggingService.LogDebug( $"Service '{result.ServiceName}' installed successfully" );
                        StatusMessage = $"Service '{result.ServiceName}' installed successfully";
                        _loggingService.LogInformation( $"Service '{result.ServiceName}' installed successfully" );

                        //service will be added via the message bus handler
                    }
                    else
                    {
                        _loggingService.LogDebug( $"Failed to install service: {result.ErrorMessage}" );
                        StatusMessage = $"Failed to install service: {result.ErrorMessage}";
                        _loggingService.LogWarning( $"Failed to install service: {result.ErrorMessage}" );
                    }
                }
                else
                {
                    _loggingService.LogWarning( "Dialog was cancelled or failed to show" );
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"ShowInstallServiceDialog threw exception: {ex.GetType().Name}" );
                _loggingService.LogError( $"Error showing install service dialog: {ex.Message}", ex );
                StatusMessage = $"Error showing dialog: {ex.Message}";

                try
                {
                    _loggingService.LogDebug( "Attempting to show error dialog" );
                    System.Windows.MessageBox.Show(
                        $"Error showing install service dialog: {ex.Message}\n\n{ex.StackTrace}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error );
                }
                catch( Exception innerEx )
                {
                    _loggingService.LogError( $"Error showing error message box: {innerEx.Message}", innerEx );
                    // Cannot even show a message box - application may be in a bad state
                    System.Diagnostics.Debug.WriteLine( $"Critical UI error: {innerEx}" );
                }
            }
        }

        private async Task RefreshSelectedServiceAsync()
        {
            if( SelectedService == null )
            {
                _loggingService.LogDebug( "RefreshSelectedServiceAsync called with no service selected" );
                return;
            }

            try
            {
                var serviceName = SelectedService.ServiceName;
                _loggingService.LogDebug( $"Refreshing service information for '{serviceName}'" );

                var updatedService = await _serviceManager.GetServiceInfoAsync( serviceName );

                if( updatedService != null )
                {
                    _loggingService.LogDebug( $"Retrieved updated information for service '{serviceName}'" );
                    //update properties of the selected service
                    SelectedService.Status = updatedService.Status;
                    SelectedService.StartupType = updatedService.StartType;
                    SelectedService.Description = updatedService.Description;
                    SelectedService.ExecutablePath = updatedService.BinaryPath;
                    SelectedService.Account = updatedService.Account;

                    _loggingService.LogDebug( $"Updated service properties: Status='{updatedService.Status}', StartType='{updatedService.StartType}'" );

                    //force UI update
                    OnPropertyChanged( nameof( SelectedService ) );
                    _loggingService.LogDebug( $"Service information updated for '{serviceName}'" );
                }
                else
                {
                    _loggingService.LogDebug( $"Service '{serviceName}' not found during refresh" );
                }
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"RefreshSelectedServiceAsync threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Error refreshing service: {ex.Message}";
                _loggingService.LogError( $"Error refreshing service information: {ex.Message}", ex );
            }
        }

        private void ShowAdminRequiredMessage( string operation )
        {
            _loggingService.LogDebug( $"Showing admin required message for operation: {operation}" );
            var message = $"Administrator privileges are required to {operation}. Would you like to restart the application as Administrator?";
            var result = System.Windows.MessageBox.Show(
                message,
                UIConstants.WarningDialogTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if( result == MessageBoxResult.Yes )
            {
                _loggingService.LogDebug( "User chose to restart as administrator" );
                RestartAsAdmin();
            }
            else
            {
                _loggingService.LogDebug( "User chose not to restart as administrator" );
            }
        }

        private void RestartAsAdmin()
        {
            _loggingService.LogDebug( "RestartAsAdmin called" );
            try
            {
                _loggingService.LogInformation( "Restarting application with administrator privileges" );

                //get the path to the current executable
                var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
                if( string.IsNullOrEmpty( currentExePath ) )
                {
                    _loggingService.LogDebug( "Failed to get current executable path" );
                    StatusMessage = "Failed to get application path. Cannot restart as administrator.";
                    return;
                }

                _loggingService.LogDebug( $"Current executable path: {currentExePath}" );

                //create a new process start info
                var startInfo = new ProcessStartInfo
                {
                    FileName = currentExePath,
                    UseShellExecute = true,
                    Verb = "runas" //this triggers the UAC prompt
                };

                _loggingService.LogDebug( "Starting new process with admin privileges" );
                Process.Start( startInfo );

                _loggingService.LogDebug( "Shutting down current application instance" );
                // Close the current application
                System.Windows.Application.Current.Shutdown();
            }
            catch( Exception ex )
            {
                _loggingService.LogDebug( $"RestartAsAdmin threw exception: {ex.GetType().Name}" );
                StatusMessage = $"Failed to restart as administrator: {ex.Message}";
                _loggingService.LogError( $"Failed to restart application with administrator privileges: {ex.Message}", ex );

                System.Windows.MessageBox.Show(
                    $"Failed to restart as administrator: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error );
            }
        }

        #region Message Handlers
        private void OnServiceInstalled( ServiceInstalledMessage message )
        {
            _loggingService.LogDebug( $"Received ServiceInstalledMessage for service: {message.ServiceName}" );
            _loggingService.LogInformation( $"Received service installed message: {message.ServiceName}" );
            RefreshServices();
        }

        private void OnServiceRemoved( ServiceRemovedMessage message )
        {
            _loggingService.LogDebug( $"Received ServiceRemovedMessage for service: {message.ServiceName}" );
            _loggingService.LogInformation( $"Received service removed message: {message.ServiceName}" );
            //service already removed from the collection in the RemoveSelectedService method
        }

        private void OnServiceChanged( ServiceChangedMessage message )
        {
            _loggingService.LogDebug( $"Received ServiceChangedMessage for service: {message.ServiceName}" );
            _loggingService.LogInformation( $"Received service changed message: {message.ServiceName}" );

            //if this is the currently selected service, refresh it
            if( SelectedService != null && SelectedService.ServiceName == message.ServiceName )
            {
                _loggingService.LogDebug( $"Refreshing selected service '{message.ServiceName}' due to change notification" );
                RefreshSelectedServiceAsync().ConfigureAwait( false );
            }
        }
        #endregion

        private bool FilterServices( object obj )
        {
            if( string.IsNullOrWhiteSpace( SearchText ) )
                return true;

            if( obj is ServiceModel service )
            {
                _loggingService.LogDebug( $"Filtering service '{service.DisplayName}' against search term '{SearchText}'" );
                return service.DisplayName.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) ||
                       service.Description.Contains( SearchText, StringComparison.OrdinalIgnoreCase ) ||
                       service.ServiceName.Contains( SearchText, StringComparison.OrdinalIgnoreCase );
            }
            return false;
        }

        private void FocusSearchBox()
        {
            _loggingService.LogDebug( "FocusSearchBox called" );
            if( System.Windows.Application.Current.MainWindow is MainWindow mainWindow )
            {
                var searchBox = mainWindow.FindName( "SearchBox" ) as System.Windows.Controls.TextBox;
                if( searchBox != null )
                {
                    _loggingService.LogDebug( "Found SearchBox control, setting focus" );
                    searchBox.Focus();
                    _loggingService.LogDebug( "Search box focused via Ctrl+F shortcut" );
                }
                else
                {
                    _loggingService.LogWarning( "SearchBox control not found" );
                }
            }
            else
            {
                _loggingService.LogWarning( "MainWindow not found or not of expected type" );
            }
        }

        private void ApplyConsoleFilter()
        {
            lock( _consoleLock )
            {
                if( string.IsNullOrWhiteSpace( _consoleSearchText ) )
                {
                    UpdateConsoleOutputWithLogLevelFilter();
                    return;
                }

                var filteredLines = _rawLogLines
                    .Where( line =>
                    {
                        //first apply log level filter
                        var passesLogLevel = _selectedLogLevel == "All" ||
                            string.IsNullOrWhiteSpace( _selectedLogLevel ) ||
                            line.Contains( $"[{_selectedLogLevel}]", StringComparison.OrdinalIgnoreCase );

                        //then apply text filter
                        var passesTextFilter = line.Contains( _consoleSearchText, StringComparison.OrdinalIgnoreCase );

                        return passesLogLevel && passesTextFilter;
                    } )
                    .ToList();

                ConsoleOutput = string.Join( Environment.NewLine, filteredLines );
            }
        }

        private void OnLogMessageReceived( object? sender, LogEventArgs e )
        {
            string logLine = $"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] [{e.Level}] {e.Message}";
            AppendToConsole( logLine );
        }

        private void AppendToConsole( string message )
        {
            if( string.IsNullOrEmpty( message ) )
                return;

            lock( _consoleLock )
            {
                _rawLogLines.Add( message );

                //trim if exceeding max number of lines
                if( _rawLogLines.Count > _maxConsoleLines )
                {
                    _rawLogLines = _rawLogLines.Skip( _rawLogLines.Count - _maxConsoleLines ).ToList();
                }

                UpdateFilteredConsoleOutput();
            }
        }

        private void UpdateFilteredConsoleOutput()
        {
            //if we have a text search filter active, use that method instead
            if( !string.IsNullOrWhiteSpace( _consoleSearchText ) )
            {
                ApplyConsoleFilter();
                return;
            }

            //otherwise just apply the log level filter
            UpdateConsoleOutputWithLogLevelFilter();
        }

        public void ClearConsole()
        {
            lock( _consoleLock )
            {
                System.Windows.Application.Current.Dispatcher.Invoke( () =>
                {
                    _rawLogLines.Clear();
                    ConsoleOutput = string.Empty;

                    //add initial messages after clearing
                    _rawLogLines.Add( $"Console cleared - {DateTime.Now}" );
                    _rawLogLines.Add( "----------------------------------------------------" );

                    UpdateFilteredConsoleOutput();
                } );
            }
        }

        private void ResetMaxConsoleLines()
        {
            _loggingService.LogDebug( "ResetMaxConsoleLines called" );
            MaxConsoleLines = 1000;
        }

        private void UpdateConsoleOutputWithLogLevelFilter()
        {
            List<string> filteredLines;

            if( _selectedLogLevel == "All" )
            {
                filteredLines = new List<string>( _rawLogLines );
            }
            else
            {
                filteredLines = _rawLogLines
                    .Where( line => line.Contains( $"[{_selectedLogLevel}]" ) )
                    .ToList();
            }

            //update the property on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke( () =>
            {
                ConsoleOutput = string.Join( '\n', filteredLines );
            } );
        }

        private void HandleAdminRequest()
        {
            if( _isAdmin )
            {
                //already running as admin, just show a message
                _loggingService.LogInformation( "Application is already running with administrator privileges" );
                StatusMessage = "Already running with administrator privileges";
                System.Windows.MessageBox.Show(
                    "The application is already running with administrator privileges.",
                    "Admin Mode Active",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information );
            }
            else
            {
                //need to restart the app as admin
                RestartAsAdmin();
            }
        }
    }
}