using Domain.Entities;

namespace Application.Abstractions;

/// <summary>
///     Exposes role management functionality across aggregate boundaries.
/// </summary>
public interface IRoleRepository
{
    Task<Role?> FindByNameAsync(string name, CancellationToken ct);
    Task EnsureSeedAsync(IEnumerable<string> roles, CancellationToken ct);
    Task AssignAsync(Guid userId, Guid roleId, CancellationToken ct);
    Task RemoveAsync(Guid userId, Guid roleId, CancellationToken ct);
    Task<string[]> GetUserRoleNamesAsync(Guid userId, CancellationToken ct);
}