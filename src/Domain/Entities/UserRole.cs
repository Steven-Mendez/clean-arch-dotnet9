namespace Domain.Entities;

/// <summary>
///     Join entity capturing the many-to-many relationship between users and roles.
/// </summary>
public sealed class UserRole
{
    private UserRole()
    {
    }

    /// <summary>
    ///     Creates a user-role link while rejecting empty identifiers.
    /// </summary>
    public UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));

        if (roleId == Guid.Empty) throw new ArgumentException("Role id is required.", nameof(roleId));

        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
}