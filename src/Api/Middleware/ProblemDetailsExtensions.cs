using Application.Common.Exceptions;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Api.Middleware;

/// <summary>
///     Configures Hellang ProblemDetails mappings for known application exceptions.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    ///     Registers exception-to-ProblemDetails translations with optional development diagnostics.
    /// </summary>
    public static IServiceCollection AddExceptionMapping(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddProblemDetails(options =>
        {
            options.IncludeExceptionDetails = (ctx, _) => environment.IsDevelopment();
            options.Map<NotFoundException>((ctx, ex) => CreateProblem(ctx, StatusCodes.Status404NotFound, ex.Message));
            options.Map<ConflictException>((ctx, ex) => CreateProblem(ctx, StatusCodes.Status409Conflict, ex.Message));
            options.Map<UnauthorizedException>((ctx, ex) =>
                CreateProblem(ctx, StatusCodes.Status401Unauthorized, ex.Message));
            options.Map<ValidationAppException>((ctx, ex) =>
                CreateProblem(ctx, StatusCodes.Status400BadRequest, ex.Message));
            options.Map<ValidationException>((ctx, ex) => CreateValidationProblem(ctx, ex));
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        });

        return services;
    }

    private static ProblemDetails CreateProblem(HttpContext context, int statusCode, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Detail = detail,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Instance = context.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblem(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };
    }
}