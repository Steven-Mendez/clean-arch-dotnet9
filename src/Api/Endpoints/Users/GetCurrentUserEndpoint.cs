using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.DTOs;
using Application.Users.Queries.GetCurrentUser;
using Cortex.Mediator;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// Maps the endpoint that returns the current authenticated user's profile.
/// </summary>
public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    /// <summary>
    /// Registers the current-user endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the current-user route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapGet("/me", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                if (!user.TryGetUserId(out var userId)) return Results.Unauthorized();

                var query = new GetCurrentUserQuery(userId);
                var result = await mediator.SendQueryAsync<GetCurrentUserQuery, UserDto>(query, ct);
                return Results.Ok(result);
            })
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName("GetCurrentUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Retrieve the current authenticated user's profile.";
                operation.Description =
                    "Looks up the user identified by the caller's JWT claims and returns their profile.";
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "Profile retrieved successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Caller is not authenticated.";

                return operation;
            });
    }
}
