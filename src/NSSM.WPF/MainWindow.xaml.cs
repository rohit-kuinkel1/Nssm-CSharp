using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NSSM.WPF.ViewModels;

namespace NSSM.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Flag to track the last focused element
    private bool _consoleHasFocus = false;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Get the view model from the App's dependency injection container
        if (App.ServiceProvider != null)
        {
            var viewModel = App.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
            if (viewModel != null)
            {
                DataContext = viewModel;
            }
        }
    }
    
    /// <summary>
    /// Event handler for the console text box text changed event.
    /// Auto-scrolls to the bottom when new text is added.
    /// </summary>
    private void ConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Automatically scroll to the bottom when text changes
        ConsoleScrollViewer.ScrollToEnd();
    }
    
    /// <summary>
    /// Validates that textbox input is numeric only
    /// </summary>
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        // Use a regular expression to only allow numbers
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
    
    /// <summary>
    /// Global keyboard event handler for the window
    /// </summary>
    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Check for Ctrl+F
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            // If the console has focus, show the console search box
            if (_consoleHasFocus)
            {
                ShowConsoleSearch();
                e.Handled = true; // Mark as handled so it doesn't trigger other Ctrl+F bindings
            }
            else
            {
                // Default to focusing the main search box
                SearchBox.Focus();
                e.Handled = true;
            }
        }
        // Check for Escape key to close the console search
        else if (e.Key == Key.Escape && ConsoleSearchPanel.Visibility == Visibility.Visible)
        {
            CloseConsoleSearch();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Shows the console search box and focuses it
    /// </summary>
    private void ShowConsoleSearch()
    {
        ConsoleSearchPanel.Visibility = Visibility.Visible;
        ConsoleSearchBox.Focus();
        ConsoleSearchBox.SelectAll();
    }

    /// <summary>
    /// Closes the console search box and clears the search
    /// </summary>
    private void CloseConsoleSearch()
    {
        // Clear the search text
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ConsoleSearchText = string.Empty;
        }
        
        // Hide the search panel
        ConsoleSearchPanel.Visibility = Visibility.Collapsed;
        
        // Return focus to console
        ConsoleTextBox.Focus();
    }

    /// <summary>
    /// Event handler for the close button on the console search box
    /// </summary>
    private void CloseConsoleSearch_Click(object sender, RoutedEventArgs e)
    {
        CloseConsoleSearch();
    }
    
    /// <summary>
    /// Tracks when the console gets focus
    /// </summary>
    private void ConsoleTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _consoleHasFocus = true;
    }
    
    /// <summary>
    /// Handle lost focus for tracking purposes
    /// </summary>
    protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnPreviewGotKeyboardFocus(e);
        
        // Update the focus tracking flag when focus changes
        _consoleHasFocus = e.NewFocus == ConsoleTextBox;
    }
}