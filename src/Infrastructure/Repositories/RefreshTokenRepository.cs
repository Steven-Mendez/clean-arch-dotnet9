using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
///     Persists refresh tokens for authentication flows.
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
    }

    public Task<RefreshToken?> FindAsync(string token, CancellationToken ct)
    {
        return _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
}