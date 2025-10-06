namespace Application.Common.Exceptions;

/// <summary>
///     Indicates that the caller is not authorized to perform the requested action.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}