using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.DTOs;
using Application.Users.Queries.GetUserById;
using Cortex.Mediator;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// Maps the endpoint that returns a specific user's profile.
/// </summary>
public sealed class GetUserByIdEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    /// <summary>
    /// Registers the user lookup endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the lookup route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapGet("/{id:guid}",
                async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var requesterId)) return Results.Unauthorized();

                    var isAdmin = user.IsInRole("Admin");
                    if (!isAdmin && requesterId != id) return Results.Forbid();

                    var query = new GetUserByIdQuery(id);
                    var result = await mediator.SendQueryAsync<GetUserByIdQuery, UserDto>(query, ct);
                    return Results.Ok(result);
                })
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetUserById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Fetch a user profile by identifier.";
                operation.Description =
                    "Allows admins to fetch any user, or individuals to fetch their own profile details.";
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "User profile found.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Caller is unauthenticated.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller lacks permission to view the requested user.";
                if (operation.Responses.TryGetValue(StatusCodes.Status404NotFound.ToString(), out var notFound))
                    notFound.Description = "No user exists with the supplied id.";

                return operation;
            });
    }
}
