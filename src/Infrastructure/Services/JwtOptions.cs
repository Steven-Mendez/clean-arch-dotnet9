namespace Infrastructure.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; }
        = 30;

    public int RefreshTokenMinutes { get; set; }
        = 43_200;
}