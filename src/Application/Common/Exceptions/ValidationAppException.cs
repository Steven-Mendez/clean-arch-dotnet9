namespace Application.Common.Exceptions;

/// <summary>
///     Indicates that input failed domain-level validation checks.
/// </summary>
public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message)
    {
    }
}