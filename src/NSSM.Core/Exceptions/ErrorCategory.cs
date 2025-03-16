namespace NSSM.Core.Exceptions
{
    /// <summary>
    /// Categorizes errors into logical groups for easier handling and reporting.
    /// </summary>
    public enum ErrorCategory
    {
        General,
        ServiceOperation,
        RegistryOperation,
        ProcessOperation,
        Configuration
    }
}
