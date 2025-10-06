using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

/// <summary>
///     Emits structured logs around command handler execution.
/// </summary>
public sealed class LoggingBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ILogger<LoggingBehavior<TCommand, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TCommand, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> Handle(TCommand command, CommandHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return HandleInternal(typeof(TCommand).Name, () => next());
    }

    private async Task<TResponse> HandleInternal(string requestName, Func<Task<TResponse>> next)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["PipelineType"] = "command"
        });

        _logger.LogInformation("Handling command {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("Handled command {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {RequestName} failed", requestName);
            throw;
        }
    }
}

/// <summary>
///     Emits structured logs around query handler execution.
/// </summary>
public sealed class LoggingQueryBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly ILogger<LoggingQueryBehavior<TQuery, TResponse>> _logger;

    public LoggingQueryBehavior(ILogger<LoggingQueryBehavior<TQuery, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> Handle(TQuery query, QueryHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return HandleInternal(typeof(TQuery).Name, () => next());
    }

    private async Task<TResponse> HandleInternal(string requestName, Func<Task<TResponse>> next)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["PipelineType"] = "query"
        });

        _logger.LogDebug("Handling query {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogDebug("Handled query {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query {RequestName} failed", requestName);
            throw;
        }
    }
}