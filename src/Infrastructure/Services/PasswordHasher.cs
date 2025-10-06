using System.Security.Cryptography;
using Application.Abstractions;

namespace Infrastructure.Services;

/// <summary>
///     Provides PBKDF2-based hashing for user passwords.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int KeySize = 64;
    private const int Iterations = 210_000;

    /// <summary>
    ///     Derives a hash and salt for the supplied password.
    /// </summary>
    public (string Hash, string Salt) Hash(string password)
    {
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be empty.", nameof(password));

        var saltBytes = new byte[SaltSize];
        RandomNumberGenerator.Fill(saltBytes);

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA512, KeySize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    ///     Verifies a password against an existing hash and salt pair.
    /// </summary>
    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;

        var saltBytes = Convert.FromBase64String(salt);
        var computedHash =
            Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA512, KeySize);
        var expectedHash = Convert.FromBase64String(hash);
        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }
}