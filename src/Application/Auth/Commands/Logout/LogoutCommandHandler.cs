using Application.Abstractions;
using Application.Common.Exceptions;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Auth.Commands.Logout;

/// <summary>
///     Revokes refresh tokens to terminate user sessions.
/// </summary>
public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _clock = clock;
    }

    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.FindAsync(command.RefreshToken, cancellationToken);
        if (token is null) return Unit.Value;

        if (token.UserId != command.UserId)
            throw new UnauthorizedException("Refresh token does not belong to the requested user.");

        if (token.RevokedAtUtc is null) token.Revoke(_clock.UtcNow);

        return Unit.Value;
    }
}