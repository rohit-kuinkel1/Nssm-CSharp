using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NSSM.WPF.ViewModels;

/// <summary>
/// Base class for all view models, providing common functionality
/// such as property change notification, validation, and busy state management.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private bool _isBusy;
    private string _busyMessage = string.Empty;

    /// <summary>
    /// Event raised when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event raised when the validation errors change
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets a value indicating whether the view model has any validation errors
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the view model is currently busy
    /// with an async operation
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty( ref _isBusy, value );
    }

    /// <summary>
    /// Gets the message describing the current busy operation
    /// </summary>
    public string BusyMessage
    {
        get => _busyMessage;
        private set => SetProperty( ref _busyMessage, value );
    }

    /// <summary>
    /// Raises the PropertyChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property that changed</param>
    protected virtual void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    /// <summary>
    /// Sets a property value and raises the PropertyChanged event if the value changed
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">New value for the property</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>True if the value changed, false otherwise</returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null
    )
    {
        if( Equals( field, value ) ) return false;
        field = value;
        OnPropertyChanged( propertyName );
        return true;
    }

    /// <summary>
    /// Sets a property value, validates it, and raises the PropertyChanged event if the value changed
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">New value for the property</param>
    /// <param name="validateValue">Validation function</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>True if the value changed, false otherwise</returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        Func<T, (bool IsValid, string ErrorMessage)> validateValue,
        [CallerMemberName] string? propertyName = null
    )
    {
        if( Equals( field, value ) ) return false;

        var (isValid, errorMessage) = validateValue( value );
        if( !isValid )
        {
            AddError( propertyName!, errorMessage );
        }
        else
        {
            RemoveErrors( propertyName! );
        }

        field = value;
        OnPropertyChanged( propertyName );
        return true;
    }

    /// <summary>
    /// Gets the validation errors for a property
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Collection of error messages</returns>
    public IEnumerable GetErrors( string? propertyName )
    {
        if( string.IsNullOrEmpty( propertyName ) || !_errors.ContainsKey( propertyName ) )
        {
            return new List<string>();
        }

        return _errors[propertyName];
    }

    /// <summary>
    /// Adds a validation error for a property
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="errorMessage">Error message</param>
    protected void AddError(
        string propertyName,
        string errorMessage
    )
    {
        if( !_errors.ContainsKey( propertyName ) )
        {
            _errors[propertyName] = new List<string>();
        }

        if( !_errors[propertyName].Contains( errorMessage ) )
        {
            _errors[propertyName].Add( errorMessage );
            OnErrorsChanged( propertyName );
        }
    }

    /// <summary>
    /// Removes all validation errors for a property
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    protected void RemoveErrors( string propertyName )
    {
        if( _errors.ContainsKey( propertyName ) )
        {
            _errors.Remove( propertyName );
            OnErrorsChanged( propertyName );
        }
    }

    /// <summary>
    /// Raises the ErrorsChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    protected void OnErrorsChanged( string propertyName )
    {
        ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propertyName ) );
        OnPropertyChanged( nameof( HasErrors ) );
    }

    /// <summary>
    /// Executes an asynchronous operation within a busy state context
    /// </summary>
    /// <typeparam name="T">Type of the result</typeparam>
    /// <param name="operation">Asynchronous operation to execute</param>
    /// <param name="busyMessage">Message to display while the operation is in progress</param>
    /// <returns>Result of the operation</returns>
    protected async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string busyMessage = "Please wait..."
    )
    {
        if( IsBusy )
        {
            throw new InvalidOperationException( "An operation is already in progress" );
        }

        try
        {
            BusyMessage = busyMessage;
            IsBusy = true;
            return await operation();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Executes an asynchronous operation within a busy state context
    /// </summary>
    /// <param name="operation">Asynchronous operation to execute</param>
    /// <param name="busyMessage">Message to display while the operation is in progress</param>
    protected async Task ExecuteAsync(
        Func<Task> operation,
        string busyMessage = "Please wait..."
    )
    {
        if( IsBusy )
        {
            throw new InvalidOperationException( "An operation is already in progress" );
        }

        try
        {
            BusyMessage = busyMessage;
            IsBusy = true;
            await operation();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }
}
