using System.Windows.Controls;
using NSSM.WPF.ViewModels;
using System.Windows;

namespace NSSM.WPF.Services
{
    /// <summary>
    /// Interface for the navigation service
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets or sets the main navigation frame
        /// </summary>
        Frame? MainFrame { get; set; }

        /// <summary>
        /// Gets a value indicating whether navigation back is possible
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Registers a view model type with a window type
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TWindow">Type of the window</typeparam>
        void RegisterWindow<TViewModel, TWindow>() where TViewModel : ViewModelBase where TWindow : Window;

        /// <summary>
        /// Registers a view model type with a page type
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TPage">Type of the page</typeparam>
        void RegisterPage<TViewModel, TPage>() where TViewModel : ViewModelBase where TPage : Page;

        /// <summary>
        /// Shows a dialog window for the specified view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <param name="ownerWindow">Owner window for the dialog</param>
        /// <returns>Dialog result</returns>
        bool? ShowDialog<TViewModel>( Window? ownerWindow = null ) where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows a dialog window for the specified view model with parameter
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <param name="parameter">Parameter to pass to the view model</param>
        /// <param name="ownerWindow">Owner window for the dialog</param>
        /// <returns>Dialog result</returns>
        bool? ShowDialog<TViewModel, TParameter>( TParameter? parameter, Window? ownerWindow = null ) where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates to a page for the specified view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates to a page for the specified view model with parameter
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <param name="parameter">Parameter to pass to the view model</param>
        void NavigateTo<TViewModel, TParameter>( TParameter? parameter ) where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates back to the previous page
        /// </summary>
        /// <returns>True if navigation was successful, false otherwise</returns>
        bool GoBack();
    }
}
