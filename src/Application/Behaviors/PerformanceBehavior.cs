using System.Diagnostics;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

/// <summary>
///     Measures command handler latency and logs when thresholds are exceeded.
/// </summary>
public sealed class PerformanceBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private const int WarningThresholdMilliseconds = 500;
    private readonly ILogger<PerformanceBehavior<TCommand, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TCommand, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> Handle(TCommand command, CommandHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return HandleInternal(typeof(TCommand).Name, () => next(), "command");
    }

    private async Task<TResponse> HandleInternal(string requestName, Func<Task<TResponse>> next, string pipelineType)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        LogDuration(requestName, pipelineType, stopwatch.ElapsedMilliseconds);
        return response;
    }

    private void LogDuration(string requestName, string pipelineType, long elapsedMilliseconds)
    {
        if (elapsedMilliseconds > WarningThresholdMilliseconds)
            _logger.LogWarning("{PipelineType} {RequestName} took {Elapsed} ms", pipelineType, requestName,
                elapsedMilliseconds);
        else
            _logger.LogDebug("{PipelineType} {RequestName} took {Elapsed} ms", pipelineType, requestName,
                elapsedMilliseconds);
    }
}

/// <summary>
///     Measures query handler latency and emits warnings when work takes too long.
/// </summary>
public sealed class PerformanceQueryBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private const int WarningThresholdMilliseconds = 250;
    private readonly ILogger<PerformanceQueryBehavior<TQuery, TResponse>> _logger;

    public PerformanceQueryBehavior(ILogger<PerformanceQueryBehavior<TQuery, TResponse>> logger)
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
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var elapsed = stopwatch.ElapsedMilliseconds;
        if (elapsed > WarningThresholdMilliseconds)
            _logger.LogWarning("query {RequestName} took {Elapsed} ms", requestName, elapsed);
        else
            _logger.LogDebug("query {RequestName} took {Elapsed} ms", requestName, elapsed);

        return response;
    }
}