using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Users.DTOs;
using Cortex.Mediator.Commands;
using Domain.Entities;
using Mapster;

namespace Application.Auth.Commands.RegisterUser;

/// <summary>
///     Handles user registration including hashing credentials and applying default roles.
/// </summary>
public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserDto>
{
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IClock clock)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<UserDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(command.Email, cancellationToken))
            throw new ConflictException("Email already registered.");

        var (hash, salt) = _passwordHasher.Hash(command.Password);
        var now = _clock.UtcNow;
        var user = User.Create(command.Email, command.DisplayName, hash, salt, now);

        await _userRepository.AddAsync(user, cancellationToken);

        var defaultRole = await _roleRepository.FindByNameAsync("User", cancellationToken)
                          ?? throw new NotFoundException("Default role 'User' is not configured.");

        await _roleRepository.AssignAsync(user.Id, defaultRole.Id, cancellationToken);

        var roles = await _roleRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        var dto = user.Adapt<UserDto>() with { Roles = roles };
        return dto;
    }
}