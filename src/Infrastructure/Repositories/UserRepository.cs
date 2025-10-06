using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
///     Entity Framework-based repository for reading and writing user aggregates.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users.AnyAsync(u => u.Email == normalized, ct);
    }

    public async Task<(IReadOnlyList<User> Users, IReadOnlyDictionary<Guid, string[]> Roles, int TotalCount)> ListAsync(
        string? emailFilter,
        string? roleFilter,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(emailFilter))
        {
            var pattern = $"%{emailFilter.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.Email, pattern));
        }

        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            var normalizedRole = roleFilter.Trim().ToLowerInvariant();
            query = query.Where(u => _context.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r)
                .Any(r => r.Name.ToLower() == normalizedRole));
        }

        query = query.OrderBy(u => u.Email);

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToArray();

        var rolesLookup = userIds.Length == 0
            ? new Dictionary<Guid, string[]>()
            : await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, role) => new { ur.UserId, role.Name })
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Name).ToArray(), ct);

        return (users, rolesLookup, totalCount);
    }
}