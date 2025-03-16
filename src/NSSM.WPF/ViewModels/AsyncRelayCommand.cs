using System.Windows.Input;

namespace NSSM.WPF.ViewModels
{
    /// <summary>
    /// Implementation of the ICommand interface that supports asynchronous operations.
    /// This is preferred for operations that involve I/O, DB access, or other potentially
    /// blocking operations.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Gets a value indicating whether the command is currently executing
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if( _isExecuting != value )
                {
                    _isExecuting = value;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the AsyncRelayCommand class
        /// </summary>
        /// <param name="execute">Function to execute when the command is invoked</param>
        /// <param name="canExecute">Predicate to determine if the command can be executed</param>
        public AsyncRelayCommand(
            Func<object?, Task> execute, 
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
        public bool CanExecute( object? parameter ) => !IsExecuting && ( _canExecute == null || _canExecute( parameter ) );

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">Parameter to pass to the execute function</param>
        public async void Execute( object? parameter )
        {
            if( !CanExecute( parameter ) )
                return;

            try
            {
                IsExecuting = true;
                await _execute( parameter );
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
