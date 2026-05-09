namespace MedCore.Modules.Identity.Domain.Tokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    
    /// <summary>
    /// Explicitly marks an existing token as modified so mutations (e.g. Revoke, MarkReplacedBy)
    /// are persisted on the next SaveChangesAsync call.
    /// Do not rely on EF change tracking alone — if this repository is reimplemented without EF
    /// (e.g. Dapper, Redis, or a remote token store), change tracking will not exist and mutations
    /// will be silently lost without this explicit call.
    /// </summary>
    Task UpdateAsync(RefreshToken token, CancellationToken ct = default);
    
    /// <summary>
    /// Revokes all refresh tokens for the given user using a bulk UPDATE statement.
    /// This bypasses EF change tracking and does not call <see cref="RefreshToken.Revoke"/>.
    /// If <see cref="RefreshToken.Revoke"/> gains side-effect logic in the future (e.g. setting
    /// a RevokedAt timestamp), this method must be updated to match.
    /// </summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
    
    Task<int> DeleteExpiredAsync(CancellationToken ct = default);
    
    Task SaveChangesAsync(CancellationToken ct = default);
}