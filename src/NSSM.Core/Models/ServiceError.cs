namespace NSSM.Core.Models;

/// <summary>
/// Contains error information from service operations
/// </summary>
public class ServiceError
{
    /// <summary>
    /// Gets or sets the Win32 error code
    /// </summary>
    public int ErrorCode { get; set; }
    
    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
} 