using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Commands.DeactivateUser;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// POST /api/v1/users/{id:guid}/deactivate - Maps the endpoint that deactivates a user account.
/// </summary>
public sealed class DeactivateUserEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Post,
        Route: "/{id:guid}/deactivate",
        Name: "DeactivateUser",
        Summary: "Deactivate a user account.",
        Description: "Marks the target user's account as inactive. Admin-only operation.");

    /// <summary>
    /// Registers the deactivate-user endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the deactivate-user route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost(Endpoint.Route,
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
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status204NoContent.ToString(), out var noContent))
                    noContent.Description = "User deactivated.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller is not an administrator.";

                return operation;
            });
    }
}
