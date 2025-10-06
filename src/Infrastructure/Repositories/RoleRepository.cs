using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
///     Provides role persistence operations backed by the relational database.
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Role?> FindByNameAsync(string name, CancellationToken ct)
    {
        var trimmed = name.Trim();
        return _context.Roles.FirstOrDefaultAsync(r => EF.Functions.ILike(r.Name, trimmed), ct);
    }

    public async Task EnsureSeedAsync(IEnumerable<string> roles, CancellationToken ct)
    {
        foreach (var role in roles)
        {
            var trimmed = role.Trim();
            var exists = await _context.Roles.AnyAsync(r => EF.Functions.ILike(r.Name, trimmed), ct);
            if (!exists) await _context.Roles.AddAsync(new Role(trimmed), ct);
        }
    }

    public async Task AssignAsync(Guid userId, Guid roleId, CancellationToken ct)
    {
        var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
        if (!exists) await _context.UserRoles.AddAsync(new UserRole(userId, roleId), ct);
    }

    public async Task RemoveAsync(Guid userId, Guid roleId, CancellationToken ct)
    {
        var entity = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
        if (entity is not null) _context.UserRoles.Remove(entity);
    }

    public Task<string[]> GetUserRoleNamesAsync(Guid userId, CancellationToken ct)
    {
        return _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (_, role) => role.Name)
            .OrderBy(name => name)
            .ToArrayAsync(ct);
    }
}