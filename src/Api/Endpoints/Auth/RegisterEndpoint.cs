using Api.Endpoints.Abstractions;
using Application.Auth.Commands.RegisterUser;
using Application.Users.DTOs;
using Cortex.Mediator;

namespace Api.Endpoints.Auth;

public sealed class RegisterEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost("/register",
                async (RegisterRequest request, IMediator mediator, CancellationToken ct) =>
                {
                    var command = new RegisterUserCommand(request.Email, request.Password, request.DisplayName);
                    var result = await mediator.SendCommandAsync<RegisterUserCommand, UserDto>(command, ct);
                    return Results.Created($"/api/v1/users/{result.Id}", result);
                })
            .AllowAnonymous()
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName);
}