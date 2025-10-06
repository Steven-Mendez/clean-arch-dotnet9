using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.DTOs;
using Application.Users.Queries.GetCurrentUser;
using Cortex.Mediator;

namespace Api.Endpoints.Users;

public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

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
            .WithName("GetCurrentUser");
    }
}