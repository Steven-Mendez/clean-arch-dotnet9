namespace Domain.Entities;

/// <summary>
///     Represents a long-lived token that allows clients to request new access tokens.
/// </summary>
public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    /// <summary>
    ///     Initializes a refresh token for the given user and expiration window.
    /// </summary>
    public RefreshToken(Guid userId, string token, DateTime nowUtc, DateTime expiresUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresUtc <= nowUtc) throw new ArgumentException("Expiration must be in the future.", nameof(expiresUtc));

        UserId = userId;
        Token = token;
        CreatedAtUtc = nowUtc;
        ExpiresAtUtc = expiresUtc;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAtUtc { get; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    ///     Determines whether the token is still usable at the provided instant.
    /// </summary>
    public bool IsActive(DateTime nowUtc)
    {
        return RevokedAtUtc is null && nowUtc < ExpiresAtUtc;
    }

    /// <summary>
    ///     Marks the token as revoked at the supplied time.
    /// </summary>
    public void Revoke(DateTime nowUtc)
    {
        RevokedAtUtc = nowUtc;
    }
}