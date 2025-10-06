using System.Security.Claims;
using Api.Endpoints.Abstractions;
using Api.Extensions;
using Application.Users.Queries.ListUsers;
using Cortex.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace Api.Endpoints.Users;

/// <summary>
/// GET /api/v1/users - Maps the endpoint that lists users with optional filters.
/// </summary>
public sealed class ListUsersEndpoint : IEndpoint
{
    public string GroupPrefix => UsersGroup.Prefix;
    public string GroupTag => UsersGroup.Tag;

    private static readonly EndpointDescriptor Endpoint = new(
        HttpVerb: HttpMethods.Get,
        Route: string.Empty,
        Name: "ListUsers",
        Summary: "List users with optional filters and pagination.",
        Description: "Admin-only listing that supports filtering by email, role, and active status, plus pagination.");

    /// <summary>
    /// Registers the list-users endpoint within the provided users <paramref name="group"/>.
    /// </summary>
    /// <param name="group">The route group to which the list-users route is added.</param>
    public void MapEndpoint(RouteGroupBuilder group)
    {
        var configuredGroup = UsersGroup.Configure(group);

        configuredGroup.MapGet(Endpoint.Route,
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
            .WithName(Endpoint.Name)
            .WithOpenApi(operation =>
            {
                operation.Summary = Endpoint.Summary;
                operation.Description = Endpoint.Description;
                if (operation.Responses.TryGetValue(StatusCodes.Status200OK.ToString(), out var okResponse))
                    okResponse.Description = "Users retrieved successfully.";
                if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var forbidden))
                    forbidden.Description = "Caller is not an administrator.";

                operation.SetParameterDescription("email", "Optional filter that matches users whose email contains the provided text.");
                operation.SetParameterDescription("role", "Optional filter that limits results to users assigned to the specified role.");
                operation.SetParameterDescription("isActive", "Optional filter that returns only active or inactive users.");
                operation.SetParameterDescription("page", "Page number starting at 1; defaults to 1 when omitted.");
                operation.SetParameterDescription("pageSize", "Number of items per page; defaults to 20.");

                return operation;
            });
    }
}

/// <summary>
/// Query parameters supported by the user listing endpoint.
/// </summary>
/// <param name="Email">Filter users whose email contains the provided value.</param>
/// <param name="Role">Filter users assigned to the specified role.</param>
/// <param name="IsActive">Filter users by active status.</param>
/// <param name="Page">Page number starting at 1.</param>
/// <param name="PageSize">Number of results per page.</param>
public sealed record ListUsersFilter(
    [FromQuery(Name = "email")] string? Email,
    [FromQuery(Name = "role")] string? Role,
    [FromQuery(Name = "isActive")] bool? IsActive,
    [FromQuery(Name = "page")] int Page = 1,
    [FromQuery(Name = "pageSize")] int PageSize = 20);
