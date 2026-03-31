namespace PatientManagementSystem.Modules.Identity.Domain.Tokens;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Returns all refresh tokens that belong to the same token family.
    /// A family groups the original token and all its rotated successors together.
    /// If any token in the family is reused after rotation, the entire family
    /// is revoked to protect against token theft.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetFamilyAsync(Guid familyId, CancellationToken ct = default);
    
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}