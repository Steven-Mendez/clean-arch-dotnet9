using Domain.Entities;

namespace Application.Abstractions;

/// <summary>
///     Encapsulates JWT issuance rules for access and refresh workflows.
/// </summary>
public interface IJwtTokenService
{
    int AccessTokenMinutes { get; }
    int RefreshTokenMinutes { get; }
    string CreateAccessToken(User user, IEnumerable<string> roles, DateTime nowUtc);
    string CreateRefreshToken();
}