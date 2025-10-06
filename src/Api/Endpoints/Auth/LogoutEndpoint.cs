using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Auth.Commands.Logout;
using Cortex.Mediator;

namespace Api.Endpoints.Auth;

public sealed class LogoutEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost("/logout",
                async (ClaimsPrincipal user, LogoutRequest request, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId)) return Results.Unauthorized();

                    var command = new LogoutCommand(userId, request.RefreshToken);
                    await mediator.SendCommandAsync<LogoutCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private sealed record LogoutRequest(string RefreshToken);
}