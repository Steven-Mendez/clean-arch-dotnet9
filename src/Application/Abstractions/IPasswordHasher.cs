namespace Application.Abstractions;

/// <summary>
///     Produces and verifies password hashes suitable for user authentication.
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);
    bool Verify(string password, string hash, string salt);
}