namespace Application.Common.Exceptions;

/// <summary>
///     Represents a domain or business conflict that prevents completing the request.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}