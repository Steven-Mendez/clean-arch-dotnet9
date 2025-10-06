namespace Application.Common.Exceptions;

/// <summary>
///     Thrown when a requested resource cannot be located.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}