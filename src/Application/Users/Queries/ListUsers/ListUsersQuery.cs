using Cortex.Mediator.Queries;

namespace Application.Users.Queries.ListUsers;

public sealed record ListUsersQuery(string? Email, string? Role, bool? IsActive, int Page = 1, int PageSize = 20)
    : IQuery<ListUsersResponse>;