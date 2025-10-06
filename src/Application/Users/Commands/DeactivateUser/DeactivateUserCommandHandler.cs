using Application.Abstractions;
using Application.Common.Exceptions;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.DeactivateUser;

/// <summary>
///     Handles deactivation requests by flipping the user status flag.
/// </summary>
public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository, IClock clock)
    {
        _userRepository = userRepository;
        _clock = clock;
    }

    public async Task<Unit> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        if (!user.IsActive) return Unit.Value;

        user.MarkDeactivated();
        user.TouchUpdated(_clock.UtcNow);

        return Unit.Value;
    }
}