using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UnitOfWork;

/// <summary>
///     Entity Framework implementation of the unit-of-work abstraction.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public EfUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<IDisposable> BeginTransactionAsync(CancellationToken ct)
    {
        var transaction = await _context.Database.BeginTransactionAsync(ct);
        return new EfUnitOfWorkTransaction(transaction);
    }

    /// <summary>
    ///     Adapts <see cref="IDbContextTransaction" /> to <see cref="IUnitOfWorkTransaction" />.
    /// </summary>
    private sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfUnitOfWorkTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct)
        {
            return _transaction.CommitAsync(ct);
        }

        public Task RollbackAsync(CancellationToken ct)
        {
            return _transaction.RollbackAsync(ct);
        }

        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}