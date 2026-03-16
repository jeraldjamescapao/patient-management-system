namespace PatientManagementSystem.Modules.Identity.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }
    
    private User() {}
    
    public User(
        string email, 
        string passwordHash, 
        string firstName, 
        string lastName, 
        bool isActive)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = isActive;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateModifiedAtUtc()
    {
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}