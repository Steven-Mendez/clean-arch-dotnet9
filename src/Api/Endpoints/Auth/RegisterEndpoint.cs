using Api.Endpoints.Abstractions;
using Application.Auth.Commands.RegisterUser;
using Application.Users.DTOs;
using Cortex.Mediator;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Auth;

/// <summary>
/// Maps the endpoint used to register new users.
/// </summary>
public sealed class RegisterEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    /// <summary>
    /// Registers the user registration endpoint within the provided auth <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the registration route is added.</param>
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
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("RegisterUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user account.";
                operation.Description =
                    "Creates a user with the provided email, password, and display name, returning the created profile.";
                if (operation.Responses.TryGetValue(StatusCodes.Status201Created.ToString(), out var created))
                    created.Description = "User successfully created.";
                if (operation.Responses.TryGetValue(StatusCodes.Status400BadRequest.ToString(), out var badRequest))
                    badRequest.Description = "Validation failed for the supplied registration payload.";
                if (operation.Responses.TryGetValue(StatusCodes.Status409Conflict.ToString(), out var conflict))
                    conflict.Description = "A user with the same email already exists.";

                return operation;
            });
    }

    /// <summary>
    /// Request payload used when registering a new user.
    /// </summary>
    /// <param name="Email">Unique email address for the new user.</param>
    /// <param name="Password">Initial password for the new user.</param>
    /// <param name="DisplayName">Human-readable display name.</param>
    private sealed record RegisterRequest(string Email, string Password, string DisplayName);
}
