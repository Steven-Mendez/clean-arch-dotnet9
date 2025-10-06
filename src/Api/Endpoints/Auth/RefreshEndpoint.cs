using Api.Endpoints.Abstractions;
using Application.Auth.Commands.RefreshJwt;
using Application.Auth.DTOs;
using Cortex.Mediator;

namespace Api.Endpoints.Auth;

public sealed class RefreshEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost("/refresh", async (RefreshRequest request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new RefreshJwtCommand(request.RefreshToken);
                var response = await mediator.SendCommandAsync<RefreshJwtCommand, AuthResponseDto>(command, ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .Produces<AuthResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private sealed record RefreshRequest(string RefreshToken);
}