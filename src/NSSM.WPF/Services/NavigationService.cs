using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NSSM.Core.Logging;
using NSSM.WPF.ViewModels;

namespace NSSM.WPF.Services
{
    /// <summary>
    /// Service responsible for navigating between views in the application.
    /// Provides methods for showing dialogs and navigating between pages in a frame.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LoggingService _loggingService;
        private readonly Dictionary<Type, Type> _viewModelToWindowMapping = new();
        private readonly Dictionary<Type, Type> _viewModelToPageMapping = new();
        private readonly Stack<object> _navigationStack = new();

        private Frame? _mainFrame;

        /// <summary>
        /// Gets or sets the main navigation frame
        /// </summary>
        public Frame? MainFrame
        {
            get => _mainFrame;
            set => _mainFrame = value;
        }

        /// <summary>
        /// Gets a value indicating whether navigation back is possible
        /// </summary>
        public bool CanGoBack => _navigationStack.Count > 1;

        /// <summary>
        /// Initializes a new instance of the NavigationService class
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving dependencies</param>
        /// <param name="loggingService">Logging service</param>
        public NavigationService(
            IServiceProvider serviceProvider,
            LoggingService loggingService
        )
        {
            _serviceProvider = serviceProvider;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Registers a view model type with a window type
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TWindow">Type of the window</typeparam>
        public void RegisterWindow<TViewModel, TWindow>() where TViewModel : ViewModelBase where TWindow : Window
        {
            _viewModelToWindowMapping[typeof( TViewModel )] = typeof( TWindow );
            _loggingService.LogDebug( $"Registered window mapping: {typeof( TViewModel ).Name} -> {typeof( TWindow ).Name}" );
        }

        /// <summary>
        /// Registers a view model type with a page type
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TPage">Type of the page</typeparam>
        public void RegisterPage<TViewModel, TPage>() where TViewModel : ViewModelBase where TPage : Page
        {
            _viewModelToPageMapping[typeof( TViewModel )] = typeof( TPage );
            _loggingService.LogDebug( $"Registered page mapping: {typeof( TViewModel ).Name} -> {typeof( TPage ).Name}" );
        }

        /// <summary>
        /// Shows a dialog window for the specified view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <param name="ownerWindow">Owner window for the dialog</param>
        /// <returns>Dialog result</returns>
        public bool? ShowDialog<TViewModel>( Window? ownerWindow = null ) where TViewModel : ViewModelBase
        {
            return ShowDialog<TViewModel, object>( null, ownerWindow );
        }

        /// <summary>
        /// Shows a dialog window for the specified view model with parameter
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <param name="parameter">Parameter to pass to the view model</param>
        /// <param name="ownerWindow">Owner window for the dialog</param>
        /// <returns>Dialog result</returns>
        public bool? ShowDialog<TViewModel, TParameter>(
            TParameter? parameter,
            Window? ownerWindow = null
        )
            where TViewModel : ViewModelBase
        {
            var viewModelType = typeof( TViewModel );

            if( !_viewModelToWindowMapping.TryGetValue( viewModelType, out var windowType ) )
            {
                _loggingService.LogError( $"No window registered for view model {viewModelType.Name}" );
                throw new InvalidOperationException( $"No window registered for view model {viewModelType.Name}" );
            }

            _loggingService.LogDebug( $"Creating dialog window for view model {viewModelType.Name}" );

            var viewModel = parameter != null
                ? _serviceProvider.GetRequiredService<TViewModel>()
                : _serviceProvider.GetRequiredService<TViewModel>();

            if( viewModel is IParameterViewModel<TParameter> parameterViewModel && parameter != null )
            {
                parameterViewModel.Initialize( parameter );
            }

            var window = (Window)_serviceProvider.GetRequiredService( windowType );
            window.DataContext = viewModel;

            if( ownerWindow != null )
            {
                window.Owner = ownerWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            _loggingService.LogDebug( $"Showing dialog window for view model {viewModelType.Name}" );
            return window.ShowDialog();
        }

        /// <summary>
        /// Navigates to a page for the specified view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            NavigateTo<TViewModel, object>( null );
        }

        /// <summary>
        /// Navigates to a page for the specified view model with parameter
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <param name="parameter">Parameter to pass to the view model</param>
        public void NavigateTo<TViewModel, TParameter>( TParameter? parameter ) where TViewModel : ViewModelBase
        {
            if( _mainFrame == null )
            {
                _loggingService.LogError( "Cannot navigate: MainFrame is not set" );
                throw new InvalidOperationException( "MainFrame is not set. Call SetMainFrame first." );
            }

            var viewModelType = typeof( TViewModel );

            if( !_viewModelToPageMapping.TryGetValue( viewModelType, out var pageType ) )
            {
                _loggingService.LogError( $"No page registered for view model {viewModelType.Name}" );
                throw new InvalidOperationException( $"No page registered for view model {viewModelType.Name}" );
            }

            _loggingService.LogDebug( $"Creating page for view model {viewModelType.Name}" );

            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            if( viewModel is IParameterViewModel<TParameter> parameterViewModel && parameter != null )
            {
                parameterViewModel.Initialize( parameter );
            }

            var page = (Page)_serviceProvider.GetRequiredService( pageType );
            page.DataContext = viewModel;

            _loggingService.LogDebug( $"Navigating to page for view model {viewModelType.Name}" );
            _mainFrame.Navigate( page );
            _navigationStack.Push( viewModel );
        }

        /// <summary>
        /// Navigates back to the previous page
        /// </summary>
        /// <returns>True if navigation was successful, false otherwise</returns>
        public bool GoBack()
        {
            if( !CanGoBack || _mainFrame == null )
            {
                return false;
            }

            _navigationStack.Pop(); //remove current
            var previousViewModel = _navigationStack.Peek(); //previous

            var viewModelType = previousViewModel.GetType();

            if( !_viewModelToPageMapping.TryGetValue( viewModelType, out var pageType ) )
            {
                _loggingService.LogError( $"No page registered for view model {viewModelType.Name}" );
                throw new InvalidOperationException( $"No page registered for view model {viewModelType.Name}" );
            }

            var page = (Page)_serviceProvider.GetRequiredService( pageType );
            page.DataContext = previousViewModel;

            _loggingService.LogDebug( $"Navigating back to page for view model {viewModelType.Name}" );
            _mainFrame.Navigate( page );

            return true;
        }
    }
}