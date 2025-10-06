namespace Application.Abstractions;

/// <summary>
///     Coordinates persistence operations across repositories within a transactional boundary.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Flushes tracked changes to the backing store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct);

    /// <summary>
    ///     Begins a transaction scope suitable for use in pipeline behaviors.
    /// </summary>
    Task<IDisposable> BeginTransactionAsync(CancellationToken ct);
}