using Application.Abstractions;
using Cortex.Mediator.Commands;

namespace Application.Behaviors;

/// <summary>
///     Wraps command execution in a unit-of-work transaction boundary.
/// </summary>
public sealed class TransactionBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TCommand command, CommandHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (transaction is IUnitOfWorkTransaction commitScope) await commitScope.CommitAsync(cancellationToken);

            return response;
        }
        catch
        {
            if (transaction is IUnitOfWorkTransaction rollbackScope)
                await rollbackScope.RollbackAsync(cancellationToken);

            throw;
        }
        finally
        {
            if (transaction is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                transaction?.Dispose();
        }
    }
}