using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Users.DTOs;
using Cortex.Mediator.Queries;
using Mapster;

namespace Application.Users.Queries.GetCurrentUser;

/// <summary>
///     Resolves the current user's profile along with role memberships.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(query.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var roles = await _roleRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        return user.Adapt<UserDto>() with { Roles = roles };
    }
}