using Domain.Entities;

namespace Application.Abstractions;

/// <summary>
///     Provides persistence operations for querying and mutating user aggregates.
/// </summary>
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);

    /// <summary>
    ///     Returns a page of users along with their role memberships and total count for filtering scenarios.
    /// </summary>
    Task<(IReadOnlyList<User> Users, IReadOnlyDictionary<Guid, string[]> Roles, int TotalCount)> ListAsync(
        string? emailFilter, string? roleFilter, bool? isActive, int page, int pageSize, CancellationToken ct);
}