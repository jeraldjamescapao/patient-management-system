namespace MedCore.Modules.Identity.Domain.Tokens;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    private RefreshToken() { }

    private RefreshToken(
        Guid userId, 
        Guid familyId, 
        string token, 
        DateTimeOffset expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        FamilyId = familyId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        IsRevoked = false;
    }

    public static RefreshToken Create(
        Guid userId, 
        Guid familyId, 
        string token, 
        DateTimeOffset expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.");
        if (familyId == Guid.Empty)
            throw new ArgumentException("FamilyId cannot be empty.");
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty.");
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            throw new ArgumentException("ExpiresAtUtc must be in the future.");
        
        return new RefreshToken(userId, familyId, token, expiresAtUtc);
    }
    
    public void Revoke()
    {
        if (IsRevoked) return;
        IsRevoked = true;
    }
    
    public void MarkReplacedBy(Guid newTokenId)
    {
        if (newTokenId == Guid.Empty)
            throw new ArgumentException("New token ID cannot be empty.");
        
        ReplacedByTokenId = newTokenId;
    }
}