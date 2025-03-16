using System.Windows.Input;

namespace NSSM.WPF.ViewModels
{
    /// <summary>
    /// Implementation of the ICommand interface that relays the Execute and CanExecute
    /// methods to delegates provided at construction.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">Action to execute when the command is invoked</param>
        /// <param name="canExecute">Predicate to determine if the command can be executed</param>
        public RelayCommand(
            Action<object?> execute,
            Predicate<object?>? canExecute = null
        )
        {
            _execute = execute ?? throw new ArgumentNullException( nameof( execute ) );
            _canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Determines if the command can be executed
        /// </summary>
        /// <param name="parameter">Parameter to use when determining if the command can be executed</param>
        /// <returns>True if the command can be executed, false otherwise</returns>
        public bool CanExecute( object? parameter ) => _canExecute == null || _canExecute( parameter );

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">Parameter to pass to the execute action</param>
        public void Execute( object? parameter ) => _execute( parameter );

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}