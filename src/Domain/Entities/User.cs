using System.Text.RegularExpressions;

namespace Domain.Entities;

/// <summary>
///     Represents an application user aggregate with invariant-enforced setters.
/// </summary>
public sealed class User
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    ///     Normalizes and assigns the user's email, validating format and emptiness.
    /// </summary>
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));

        var trimmed = email.Trim();
        if (!EmailRegex.IsMatch(trimmed)) throw new ArgumentException("Email format is invalid.", nameof(email));

        Email = trimmed.ToLowerInvariant();
    }

    /// <summary>
    ///     Applies trimming and length validation to the user's display name before assignment.
    /// </summary>
    public void SetDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Display name cannot be empty.", nameof(name));

        var trimmed = name.Trim();
        if (trimmed.Length is < 2 or > 100)
            throw new ArgumentException("Display name must be between 2 and 100 characters.", nameof(name));

        DisplayName = trimmed;
    }

    /// <summary>
    ///     Persists the password hash and salt after checking for missing values.
    /// </summary>
    public void SetPassword(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(hash));

        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Password salt cannot be empty.", nameof(salt));

        PasswordHash = hash;
        PasswordSalt = salt;
    }

    public void MarkDeactivated()
    {
        IsActive = false;
    }

    /// <summary>
    ///     Updates the <see cref="UpdatedAtUtc" /> marker when mutations occur.
    /// </summary>
    public void TouchUpdated(DateTime nowUtc)
    {
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>
    ///     Factory helper that constructs a user with pre-initialized audit fields and validated data.
    /// </summary>
    public static User Create(string email, string displayName, string passwordHash, string passwordSalt,
        DateTime nowUtc)
    {
        var user = new User
        {
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        user.SetEmail(email);
        user.SetDisplayName(displayName);
        user.SetPassword(passwordHash, passwordSalt);

        return user;
    }
}