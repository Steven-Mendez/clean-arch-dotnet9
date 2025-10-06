using Api.Endpoints.Abstractions;
using Application.Auth.Commands.RefreshJwt;
using Application.Auth.DTOs;
using Cortex.Mediator;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Auth;

/// <summary>
/// Maps the endpoint that exchanges refresh tokens for new JWTs.
/// </summary>
public sealed class RefreshEndpoint : IEndpoint
{
    public string GroupPrefix => AuthGroup.Prefix;
    public string GroupTag => AuthGroup.Tag;

    /// <summary>
    /// Registers the token refresh endpoint within the provided auth <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the refresh route is added.</param>
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName("RefreshTokens")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Rotate JWT tokens using a valid refresh token.";
                operation.Description =
                    "Takes a refresh token and returns a fresh access token plus a new refresh token.";
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "Tokens successfully refreshed.";
                if (operation.Responses.TryGetValue(StatusCodes.Status400BadRequest.ToString(), out var badRequest))
                    badRequest.Description = "The refresh token payload was malformed.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Refresh token is invalid, revoked, or expired.";

                return operation;
            });
    }

    /// <summary>
    /// Request payload containing the refresh token to exchange.
    /// </summary>
    /// <param name="RefreshToken">Refresh token previously issued to the client.</param>
    private sealed record RefreshRequest(string RefreshToken);
}
