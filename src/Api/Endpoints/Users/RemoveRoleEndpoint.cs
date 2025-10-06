using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Commands.RemoveRole;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// Maps the endpoint that allows administrators to remove a role from a user.
/// </summary>
public sealed class RemoveRoleEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    /// <summary>
    /// Registers the remove-role endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the remove-role route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapDelete("/{id:guid}/roles",
                async (Guid id, [FromBody] RoleMutationRequest request, ClaimsPrincipal user, IMediator mediator,
                    CancellationToken ct) =>
                {
                    if (!user.IsInRole("Admin")) return Results.Forbid();

                    var command = new RemoveRoleCommand(id, request.RoleName);
                    await mediator.SendCommandAsync<RemoveRoleCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("RemoveRole")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Remove a role assignment from a user.";
                operation.Description = "Allows administrators to revoke a specific role from the target user.";
                if (operation.Responses.TryGetValue(StatusCodes.Status204NoContent.ToString(), out var noContent))
                    noContent.Description = "Role removed successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller is not an administrator.";

                return operation;
            });
    }
}
