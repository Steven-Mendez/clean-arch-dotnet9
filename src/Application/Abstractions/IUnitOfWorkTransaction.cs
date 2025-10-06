namespace Application.Abstractions;

/// <summary>
///     Represents an active transaction that can be completed or cancelled explicitly.
/// </summary>
public interface IUnitOfWorkTransaction : IAsyncDisposable, IDisposable
{
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}