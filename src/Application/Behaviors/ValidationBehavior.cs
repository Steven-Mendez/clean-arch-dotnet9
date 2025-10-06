using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

/// <summary>
///     Runs FluentValidation validators before command handlers execute.
/// </summary>
public sealed class ValidationBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ILogger<ValidationBehavior<TCommand, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TCommand>> validators,
        ILogger<ValidationBehavior<TCommand, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public Task<TResponse> Handle(TCommand command, CommandHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return HandleInternal(command, () => next(), cancellationToken);
    }

    private async Task<TResponse> HandleInternal(TCommand request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        _logger.LogDebug("Running validation for {RequestType}", typeof(TCommand).Name);

        var context = new ValidationContext<TCommand>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors.Where(e => e is not null));
        }

        if (failures.Count != 0) throw new ValidationException(failures);

        return await next();
    }
}

/// <summary>
///     Runs FluentValidation validators before query handlers execute.
/// </summary>
public sealed class ValidationQueryBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly ILogger<ValidationQueryBehavior<TQuery, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TQuery>> _validators;

    public ValidationQueryBehavior(IEnumerable<IValidator<TQuery>> validators,
        ILogger<ValidationQueryBehavior<TQuery, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public Task<TResponse> Handle(TQuery query, QueryHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return HandleInternal(query, () => next(), cancellationToken);
    }

    private async Task<TResponse> HandleInternal(TQuery request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        _logger.LogDebug("Running validation for {RequestType}", typeof(TQuery).Name);

        var context = new ValidationContext<TQuery>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors.Where(e => e is not null));
        }

        if (failures.Count != 0) throw new ValidationException(failures);

        return await next();
    }
}