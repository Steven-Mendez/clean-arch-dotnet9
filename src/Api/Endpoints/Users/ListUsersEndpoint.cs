using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Application.Users.Queries.ListUsers;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Users;

public sealed class ListUsersEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapGet(string.Empty,
                async ([AsParameters] ListUsersFilter filter, ClaimsPrincipal user, IMediator mediator,
                    CancellationToken ct) =>
                {
                    if (!user.IsInRole("Admin")) return Results.Forbid();

                    var query = new ListUsersQuery(filter.Email, filter.Role, filter.IsActive, filter.Page,
                        filter.PageSize);
                    var result = await mediator.SendQueryAsync<ListUsersQuery, ListUsersResponse>(query, ct);
                    return Results.Ok(result);
                })
            .Produces<ListUsersResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("ListUsers");
    }
}

public sealed record ListUsersFilter(
    [FromQuery(Name = "email")] string? Email,
    [FromQuery(Name = "role")] string? Role,
    [FromQuery(Name = "isActive")] bool? IsActive,
    [FromQuery(Name = "page")] int Page = 1,
    [FromQuery(Name = "pageSize")] int PageSize = 20);