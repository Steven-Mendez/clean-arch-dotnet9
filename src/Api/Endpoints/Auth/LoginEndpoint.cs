using Api.Endpoints.Abstractions;
using Application.Auth.Commands.LoginUser;
using Application.Auth.DTOs;
using Cortex.Mediator;

namespace Api.Endpoints.Auth;

public sealed class LoginEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost("/login", async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new LoginUserCommand(request.Email, request.Password);
                var response = await mediator.SendCommandAsync<LoginUserCommand, AuthResponseDto>(command, ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .Produces<AuthResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private sealed record LoginRequest(string Email, string Password);
}