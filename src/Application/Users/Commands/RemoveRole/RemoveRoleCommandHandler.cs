using Application.Abstractions;
using Application.Common.Exceptions;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.RemoveRole;

/// <summary>
///     Removes a role assignment from a user when present.
/// </summary>
public sealed class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public RemoveRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IClock clock)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _clock = clock;
    }

    public async Task<Unit> Handle(RemoveRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var role = await _roleRepository.FindByNameAsync(command.RoleName, cancellationToken)
                   ?? throw new NotFoundException("Role not found.");

        await _roleRepository.RemoveAsync(user.Id, role.Id, cancellationToken);
        user.TouchUpdated(_clock.UtcNow);

        return Unit.Value;
    }
}