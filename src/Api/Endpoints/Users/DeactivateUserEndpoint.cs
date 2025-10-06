using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Commands.DeactivateUser;
using Cortex.Mediator;

namespace Api.Endpoints.Users;

public sealed class DeactivateUserEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost("/{id:guid}/deactivate",
                async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.IsInRole("Admin")) return Results.Forbid();

                    var command = new DeactivateUserCommand(id);
                    await mediator.SendCommandAsync<DeactivateUserCommand, Unit>(command, ct);
                    return Results.NoContent();
                })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("DeactivateUser");
    }
}