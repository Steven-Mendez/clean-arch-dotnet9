using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Auth.Commands.Logout;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Auth;

/// <summary>
/// POST /api/v1/auth/logout - Maps the endpoint that revokes the caller's refresh token.
/// </summary>
public sealed class LogoutEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Post,
        Route: "/logout",
        Name: "LogoutUser",
        Summary: "Invalidate the caller's refresh token.",
        Description: "Revokes the provided refresh token so it can no longer be used.");

    /// <summary>
    /// Registers the logout endpoint within the provided auth <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the logout route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost(Endpoint.Route,
                async (ClaimsPrincipal user, LogoutRequest request, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId)) return Results.Unauthorized();

                    var command = new LogoutCommand(userId, request.RefreshToken);
                    await mediator.SendCommandAsync<LogoutCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status204NoContent.ToString(), out var noContent))
                    noContent.Description = "Logout completed; refresh token revoked.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Caller is unauthenticated or the supplied token is invalid.";

                operation.SetRequestBodyDescription("Refresh token payload identifying which token to revoke.", required: true);

                return operation;
            });
    }

    /// <summary>
    /// Request payload containing the refresh token to revoke.
    /// </summary>
    /// <param name="RefreshToken">Refresh token that should be invalidated.</param>
    private sealed record LogoutRequest(string RefreshToken);
}
