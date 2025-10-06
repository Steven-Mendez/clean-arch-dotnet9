using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Commands.RemoveRole;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Users;

public sealed class RemoveRoleEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

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
            .WithName("RemoveRole");
    }
}