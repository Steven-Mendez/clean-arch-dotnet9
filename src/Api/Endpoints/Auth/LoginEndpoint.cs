using Api.Endpoints.Abstractions;
using Application.Auth.Commands.LoginUser;
using Application.Auth.DTOs;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Auth;

/// <summary>
/// POST /api/v1/auth/login - Maps the authentication endpoint responsible for user login.
/// </summary>
public sealed class LoginEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Post,
        Route: "/login",
        Name: "LoginUser",
        Summary: "Authenticate a user and issue JWT tokens.",
        Description: "Accepts credentials and returns access and refresh tokens for authenticated requests.");

    /// <summary>
    /// Registers the login endpoint within the provided auth <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the login route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = AuthGroup.Configure(group);

        configuredGroup.MapPost(Endpoint.Route, async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new LoginUserCommand(request.Email, request.Password);
                var response = await mediator.SendCommandAsync<LoginUserCommand, AuthResponseDto>(command, ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .Produces<AuthResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "Authentication succeeded; tokens returned.";
                if (operation.Responses.TryGetValue(StatusCodes.Status400BadRequest.ToString(), out var badRequest))
                    badRequest.Description = "Validation failed for the supplied credentials.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Credentials were incorrect or the account is inactive.";

                return operation;
            });
    }

    /// <summary>
    /// Request payload for logging in a user.
    /// </summary>
    /// <param name="Email">Account email address.</param>
    /// <param name="Password">Account password.</param>
    private sealed record LoginRequest(string Email, string Password);
}
