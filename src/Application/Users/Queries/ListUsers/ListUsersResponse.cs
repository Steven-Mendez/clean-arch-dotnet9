using Application.Users.DTOs;

namespace Application.Users.Queries.ListUsers;

public sealed record ListUsersResponse(IReadOnlyList<UserDto> Users, int TotalCount, int Page, int PageSize);