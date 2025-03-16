using System.Windows;

namespace NSSM.WPF.Services
{
    /// <summary>
    /// Interface for the dialog service.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a message dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="buttons">The buttons to display</param>
        /// <param name="icon">The icon to display</param>
        /// <returns>The result of the dialog</returns>
        MessageBoxResult ShowMessage(
            string message,
            string title,
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None
        );

        /// <summary>
        /// Shows a confirmation dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The title of the dialog</param>
        /// <returns>True if the user confirmed, false otherwise</returns>
        bool Confirm(
            string message,
            string title = "Confirmation"
        );

        /// <summary>
        /// Shows an error dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The error message to display</param>
        /// <param name="title">The title of the dialog</param>
        void ShowError(
            string message,
            string title = "Error"
        );

        /// <summary>
        /// Shows a warning dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The warning message to display</param>
        /// <param name="title">The title of the dialog</param>
        void ShowWarning(
            string message,
            string title = "Warning"
        );

        /// <summary>
        /// Shows an information dialog with the specified text and title.
        /// </summary>
        /// <param name="message">The information message to display</param>
        /// <param name="title">The title of the dialog</param>
        void ShowInfo(
            string message,
            string title = "Information"
        );

        /// <summary>
        /// Shows a file picker dialog for opening a file.
        /// </summary>
        /// <param name="filter">The file filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <returns>The selected file path, or null if canceled</returns>
        string? ShowOpenFileDialog(
            string filter,
            string? initialDirectory = null
        );

        /// <summary>
        /// Shows a file picker dialog for saving a file.
        /// </summary>
        /// <param name="filter">The file filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <param name="defaultFileName">The default file name to suggest</param>
        /// <returns>The selected file path, or null if canceled</returns>
        string? ShowSaveFileDialog(
            string filter,
            string? initialDirectory = null,
            string? defaultFileName = null
        );

        /// <summary>
        /// Shows a folder picker dialog.
        /// </summary>
        /// <param name="initialDirectory">The initial directory to show</param>
        /// <returns>The selected folder path, or null if canceled</returns>
        string? ShowFolderBrowserDialog( string? initialDirectory = null );
    }
}
