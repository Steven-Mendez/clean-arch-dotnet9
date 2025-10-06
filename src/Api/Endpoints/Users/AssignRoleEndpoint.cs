using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.Commands.AssignRole;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// POST /api/v1/users/{id:guid}/roles - Maps the endpoint that assigns a role to a user.
/// </summary>
public sealed class AssignRoleEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Post,
        Route: "/{id:guid}/roles",
        Name: "AssignRole",
        Summary: "Assign a role to a user.",
        Description: "Restricts access to administrators to grant an additional role.");

    /// <summary>
    /// Registers the assign-role endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the assign-role route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapPost(Endpoint.Route,
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
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status204NoContent.ToString(), out var noContent))
                    noContent.Description = "Role assigned successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller is not authorized to manage roles.";

                operation.SetParameterDescription("id", "Unique identifier of the user receiving the role.", required: true);
                operation.SetRequestBodyDescription("Role assignment payload specifying the role to grant.", required: true);

                return operation;
            });
    }
}
