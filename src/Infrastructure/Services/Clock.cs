using Application.Abstractions;

namespace Infrastructure.Services;

/// <summary>
///     System-backed implementation of <see cref="IClock" />.
/// </summary>
public sealed class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}