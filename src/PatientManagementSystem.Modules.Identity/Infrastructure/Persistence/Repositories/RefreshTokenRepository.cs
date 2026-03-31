namespace PatientManagementSystem.Modules.Identity.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using PatientManagementSystem.Modules.Identity.Domain.Tokens;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenRepository(IdentityDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<RefreshToken>> GetFamilyAsync(Guid familyId, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .Where(t => t.FamilyId == familyId)
            .ToListAsync(ct);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);
    }
    
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId)
            .ExecuteUpdateAsync(u => 
                u.SetProperty(t => t.IsRevoked, true), cancellationToken: ct);
    }

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        await _context.RefreshTokens
            .Where(t => t.ExpiresAtUtc < DateTimeOffset.UtcNow)
            .ExecuteDeleteAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}