using Application.Abstractions;
using Application.Common.Exceptions;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.ChangePassword;

/// <summary>
///     Handles password changes, enforcing authorization and current password checks.
/// </summary>
public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IClock clock)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(command.TargetUserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        if (!user.IsActive) throw new ConflictException("Cannot change password for a deactivated user.");

        var isSelf = command.RequestingUserId == user.Id;
        if (!command.IsAdmin && !isSelf)
            throw new UnauthorizedException("Requesting user cannot change another user's password.");

        if (!command.IsAdmin)
            if (string.IsNullOrEmpty(command.CurrentPassword) ||
                !_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedException("Current password is invalid.");

        var (hash, salt) = _passwordHasher.Hash(command.NewPassword);
        user.SetPassword(hash, salt);
        user.TouchUpdated(_clock.UtcNow);

        return Unit.Value;
    }
}