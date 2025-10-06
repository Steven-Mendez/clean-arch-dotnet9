using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Commands.AssignRole;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Users;

public sealed class AssignRoleEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost("/{id:guid}/roles",
                async (Guid id, [FromBody] RoleMutationRequest request, ClaimsPrincipal user, IMediator mediator,
                    CancellationToken ct) =>
                {
                    if (!user.IsInRole("Admin")) return Results.Forbid();

                    var command = new AssignRoleCommand(id, request.RoleName);
                    await mediator.SendCommandAsync<AssignRoleCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("AssignRole");
    }
}