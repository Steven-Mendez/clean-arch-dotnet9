using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Users.DTOs;
using Cortex.Mediator.Queries;
using Mapster;

namespace Application.Users.Queries.GetUserById;

/// <summary>
///     Retrieves an arbitrary user's profile by identifier.
/// </summary>
public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(query.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var roles = await _roleRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        return user.Adapt<UserDto>() with { Roles = roles };
    }
}