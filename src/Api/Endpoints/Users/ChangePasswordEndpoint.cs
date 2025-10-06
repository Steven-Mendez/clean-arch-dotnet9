using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.Commands.ChangePassword;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// POST /api/v1/users/{id:guid}/password - Maps the endpoint that updates a user's password.
/// </summary>
public sealed class ChangePasswordEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Post,
        Route: "/{id:guid}/password",
        Name: "ChangePassword",
        Summary: "Update a user's password.",
        Description: "Requires the caller to own the account or be an admin. Validates the current password when provided.");

    /// <summary>
    /// Registers the change-password endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the change-password route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost(Endpoint.Route,
                async (Guid id, [FromBody] ChangePasswordRequest request, ClaimsPrincipal user, IMediator mediator,
                    CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var requesterId)) return Results.Unauthorized();

                    var isAdmin = user.IsInRole("Admin");
                    if (!isAdmin && requesterId != id) return Results.Forbid();

                    var command = new ChangePasswordCommand(id, requesterId, request.CurrentPassword,
                        request.NewPassword, isAdmin);
                    await mediator.SendCommandAsync<ChangePasswordCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status204NoContent.ToString(), out var noContent))
                    noContent.Description = "Password updated successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Caller is unauthenticated.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller cannot modify the requested user's password.";

                operation.SetParameterDescription("id", "User identifier whose password is being updated.", required: true);
                operation.SetRequestBodyDescription("Payload containing the current (when required) and new password.", required: true);

                return operation;
            });
    }

    /// <summary>
    /// Request payload used to change a user's password.
    /// </summary>
    /// <param name="CurrentPassword">The caller's current password; optional for admins.</param>
    /// <param name="NewPassword">The desired new password.</param>
    private sealed record ChangePasswordRequest(string? CurrentPassword, string NewPassword);
}
