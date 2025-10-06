using Domain.Entities;

namespace Application.Abstractions;

/// <summary>
///     Handles persistence of refresh token lifecycle operations.
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct);
    Task<RefreshToken?> FindAsync(string token, CancellationToken ct);
}