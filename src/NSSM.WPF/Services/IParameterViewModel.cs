namespace NSSM.WPF.Services
{
    /// <summary>
    /// Interface for a view model that accepts a parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter</typeparam>
    public interface IParameterViewModel<in T>
    {
        /// <summary>
        /// Initializes the view model with a parameter
        /// </summary>
        /// <param name="parameter">Parameter to use for initialization</param>
        void Initialize( T parameter );
    }
}
