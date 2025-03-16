using System.Windows;
using NSSM.Core.Logging;

namespace NSSM.WPF.Services
{
    /// <summary>
    /// Service for displaying dialog boxes and file dialogs.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the DialogService class.
        /// </summary>
        /// <param name="loggingService">Logging service for diagnostic information</param>
        public DialogService( LoggingService loggingService )
        {
            _loggingService = loggingService;
        }

        /// <summary>
        /// Shows a message dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="buttons">The buttons to display</param>
        /// <param name="icon">The icon to display</param>
        /// <returns>The result of the dialog</returns>
        public MessageBoxResult ShowMessage(
            string message,
            string title,
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None
        )
        {
            _loggingService.LogDebug( $"Showing message dialog: {title} - {message}" );
            return System.Windows.MessageBox.Show( message, title, buttons, icon );
        }

        /// <summary>
        /// Shows a confirmation dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The title of the dialog</param>
        /// <returns>True if the user confirmed, false otherwise</returns>
        public bool Confirm(
            string message,
            string title = "Confirmation"
        )
        {
            _loggingService.LogDebug( $"Showing confirmation dialog: {title} - {message}" );
            return System.Windows.MessageBox.Show( message, title, MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Shows an error dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The error message to display</param>
        /// <param name="title">The title of the dialog</param>
        public void ShowError(
            string message,
            string title = "Error"
        )
        {
            _loggingService.LogDebug( $"Showing error dialog: {title} - {message}" );
            System.Windows.MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Error );
        }

        /// <summary>
        /// Shows a warning dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The warning message to display</param>
        /// <param name="title">The title of the dialog</param>
        public void ShowWarning(
            string message,
            string title = "Warning"
        )
        {
            _loggingService.LogDebug( $"Showing warning dialog: {title} - {message}" );
            System.Windows.MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Warning );
        }

        /// <summary>
        /// Shows an information dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The information message to display</param>
        /// <param name="title">The title of the dialog</param>
        public void ShowInfo(
            string message,
            string title = "Information"
        )
        {
            _loggingService.LogDebug( $"Showing info dialog: {title} - {message}" );
            System.Windows.MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Information );
        }

        /// <summary>
        /// Shows a file picker dialog for opening a file.
        /// </summary>
        /// <param name="filter">The file filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <returns>The selected file path, or null if canceled</returns>
        public string? ShowOpenFileDialog(
            string filter,
            string? initialDirectory = null
        )
        {
            _loggingService.LogDebug( $"Showing open file dialog with filter: {filter}" );

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                CheckFileExists = true,
                Multiselect = false
            };

            if( !string.IsNullOrEmpty( initialDirectory ) )
            {
                dialog.InitialDirectory = initialDirectory;
            }

            if( dialog.ShowDialog() == true )
            {
                _loggingService.LogDebug( $"Selected file: {dialog.FileName}" );
                return dialog.FileName;
            }

            _loggingService.LogDebug( "File selection canceled" );
            return null;
        }

        /// <summary>
        /// Shows a file picker dialog for saving a file.
        /// </summary>
        /// <param name="filter">The file filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <param name="defaultFileName">The default file name to suggest</param>
        /// <returns>The selected file path, or null if canceled</returns>
        public string? ShowSaveFileDialog(
            string filter,
            string? initialDirectory = null,
            string? defaultFileName = null
        )
        {
            _loggingService.LogDebug( $"Showing save file dialog with filter: {filter}" );

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                CheckPathExists = true
            };

            if( !string.IsNullOrEmpty( initialDirectory ) )
            {
                dialog.InitialDirectory = initialDirectory;
            }

            if( !string.IsNullOrEmpty( defaultFileName ) )
            {
                dialog.FileName = defaultFileName;
            }

            if( dialog.ShowDialog() == true )
            {
                _loggingService.LogDebug( $"Selected file: {dialog.FileName}" );
                return dialog.FileName;
            }

            _loggingService.LogDebug( "File selection canceled" );
            return null;
        }

        /// <summary>
        /// Shows a folder picker dialog.
        /// </summary>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <returns>The selected folder path, or null if canceled</returns>
        public string? ShowFolderBrowserDialog( string? initialDirectory = null )
        {
            _loggingService.LogDebug( "Showing folder browser dialog" );

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if( !string.IsNullOrEmpty( initialDirectory ) )
            {
                dialog.SelectedPath = initialDirectory;
            }

            if( dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                _loggingService.LogDebug( $"Selected folder: {dialog.SelectedPath}" );
                return dialog.SelectedPath;
            }

            _loggingService.LogDebug( "Folder selection canceled" );
            return null;
        }
    }
}