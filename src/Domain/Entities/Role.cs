namespace Domain.Entities;

/// <summary>
///     Represents a named role that can be associated with one or more users.
/// </summary>
public sealed class Role
{
    private Role()
    {
    }

    /// <summary>
    ///     Creates a role ensuring the provided name is non-empty and trimmed.
    /// </summary>
    public Role(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name.Trim();
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
}