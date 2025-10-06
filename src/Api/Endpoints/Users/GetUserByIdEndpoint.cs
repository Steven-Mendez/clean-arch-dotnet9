using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.DTOs;
using Application.Users.Queries.GetUserById;
using Cortex.Mediator;

namespace Api.Endpoints.Users;

public sealed class GetUserByIdEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

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
            .WithName("GetUserById");
    }
}