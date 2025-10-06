using Application.Abstractions;
using Application.Auth.DTOs;
using Application.Common.Exceptions;
using Application.Users.DTOs;
using Cortex.Mediator.Commands;
using Domain.Entities;
using Mapster;

namespace Application.Auth.Commands.LoginUser;

/// <summary>
///     Authenticates users, issues JWT tokens, and stores refresh tokens.
/// </summary>
public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IClock _clock;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IClock clock)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _clock = clock;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email, cancellationToken)
                   ?? throw new UnauthorizedException("Invalid credentials.");

        if (!user.IsActive) throw new UnauthorizedException("User account is deactivated.");

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedException("Invalid credentials.");

        var roles = await _roleRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        var now = _clock.UtcNow;

        var accessToken = _jwtTokenService.CreateAccessToken(user, roles, now);
        var refreshTokenValue = _jwtTokenService.CreateRefreshToken();
        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, now,
            now.AddMinutes(_jwtTokenService.RefreshTokenMinutes));

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var userDto = user.Adapt<UserDto>() with { Roles = roles };
        return new AuthResponseDto(accessToken, refreshTokenValue, userDto);
    }
}