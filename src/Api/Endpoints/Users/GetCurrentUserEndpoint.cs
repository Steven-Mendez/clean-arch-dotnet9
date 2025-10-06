using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.DTOs;
using Application.Users.Queries.GetCurrentUser;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// GET /api/v1/users/me - Maps the endpoint that returns the current authenticated user's profile.
/// </summary>
public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Get,
        Route: "/me",
        Name: "GetCurrentUser",
        Summary: "Retrieve the current authenticated user's profile.",
        Description: "Looks up the user identified by the caller's JWT claims and returns their profile.");

    /// <summary>
    /// Registers the current-user endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the current-user route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapGet(Endpoint.Route, async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                if (!user.TryGetUserId(out var userId)) return Results.Unauthorized();

                var query = new GetCurrentUserQuery(userId);
                var result = await mediator.SendQueryAsync<GetCurrentUserQuery, UserDto>(query, ct);
                return Results.Ok(result);
            })
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "Profile retrieved successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var unauthorized))
                    unauthorized.Description = "Caller is not authenticated.";

                return operation;
            });
    }
}
