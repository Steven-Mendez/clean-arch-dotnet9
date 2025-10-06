using Application.Abstractions;
using Application.Users.DTOs;
using Cortex.Mediator.Queries;
using Mapster;

namespace Application.Users.Queries.ListUsers;

/// <summary>
///     Provides filtered, paginated listings of users projected into DTOs.
/// </summary>
public sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, ListUsersResponse>
{
    private readonly IUserRepository _userRepository;

    public ListUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ListUsersResponse> Handle(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var (users, rolesMap, totalCount) = await _userRepository.ListAsync(
            query.Email,
            query.Role,
            query.IsActive,
            query.Page,
            query.PageSize,
            cancellationToken);

        var dtos = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            rolesMap.TryGetValue(user.Id, out var roles);
            var dto = user.Adapt<UserDto>() with { Roles = roles ?? Array.Empty<string>() };
            dtos.Add(dto);
        }

        return new ListUsersResponse(dtos, totalCount, query.Page, query.PageSize);
    }
}