using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.Commands.ChangePassword;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Users;

public sealed class ChangePasswordEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost("/{id:guid}/password",
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
            .WithName("ChangePassword");
    }

    private sealed record ChangePasswordRequest(string? CurrentPassword, string NewPassword);
}