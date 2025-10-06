using Application.Abstractions;
using Application.Common.Exceptions;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.AssignRole;

/// <summary>
///     Assigns an existing role to a user and updates audit metadata.
/// </summary>
public sealed class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public AssignRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IClock clock)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _clock = clock;
    }

    public async Task<Unit> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var role = await _roleRepository.FindByNameAsync(command.RoleName, cancellationToken)
                   ?? throw new NotFoundException("Role not found.");

        await _roleRepository.AssignAsync(user.Id, role.Id, cancellationToken);
        user.TouchUpdated(_clock.UtcNow);

        return Unit.Value;
    }
}