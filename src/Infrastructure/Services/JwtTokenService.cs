using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
///     Implements JWT issuance for the authentication pipeline using configured options.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.Key))
            throw new ArgumentException("JWT signing key is not configured.", nameof(options));
    }

    public int AccessTokenMinutes => _options.AccessTokenMinutes;

    public int RefreshTokenMinutes => _options.RefreshTokenMinutes;

    /// <summary>
    ///     Builds a signed JWT containing the supplied user's identity and role claims.
    /// </summary>
    public string CreateAccessToken(User user, IEnumerable<string> roles, DateTime nowUtc)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_options.Key);
        var signingCredentials =
            new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var expiry = nowUtc.AddMinutes(_options.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            nowUtc,
            expiry,
            signingCredentials);

        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    ///     Generates a cryptographically strong refresh token string.
    /// </summary>
    public string CreateRefreshToken()
    {
        Span<byte> buffer = stackalloc byte[64];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer);
    }
}