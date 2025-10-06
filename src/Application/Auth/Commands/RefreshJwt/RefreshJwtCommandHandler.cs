using Application.Abstractions;
using Application.Auth.DTOs;
using Application.Common.Exceptions;
using Application.Users.DTOs;
using Cortex.Mediator.Commands;
using Domain.Entities;
using Mapster;

namespace Application.Auth.Commands.RefreshJwt;

/// <summary>
///     Exchanges valid refresh tokens for new access credentials while rotating refresh tokens.
/// </summary>
public sealed class RefreshJwtCommandHandler : ICommandHandler<RefreshJwtCommand, AuthResponseDto>
{
    private readonly IClock _clock;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public RefreshJwtCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService,
        IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtTokenService = jwtTokenService;
        _clock = clock;
    }

    public async Task<AuthResponseDto> Handle(RefreshJwtCommand command, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.FindAsync(command.RefreshToken, cancellationToken)
                          ?? throw new UnauthorizedException("Invalid refresh token.");

        var now = _clock.UtcNow;
        if (!storedToken.IsActive(now)) throw new UnauthorizedException("Refresh token is expired or revoked.");

        var user = await _userRepository.FindByIdAsync(storedToken.UserId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found for token.");

        if (!user.IsActive) throw new UnauthorizedException("User account is deactivated.");

        var roles = await _roleRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenService.CreateAccessToken(user, roles, now);

        storedToken.Revoke(now);

        var newRefreshTokenValue = _jwtTokenService.CreateRefreshToken();
        var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenValue, now,
            now.AddMinutes(_jwtTokenService.RefreshTokenMinutes));
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        var userDto = user.Adapt<UserDto>() with { Roles = roles };
        return new AuthResponseDto(accessToken, newRefreshTokenValue, userDto);
    }
}