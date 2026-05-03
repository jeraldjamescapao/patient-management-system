namespace MedCore.Modules.Identity.Domain.Tokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    
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