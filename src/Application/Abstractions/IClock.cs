namespace Application.Abstractions;

/// <summary>
///     Abstracts access to the current time to simplify deterministic testing.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}