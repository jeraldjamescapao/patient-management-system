namespace PatientManagementSystem.Modules.Identity.Domain.Users;

using Microsoft.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public string FullNameWithInitials => $"{FirstName[..1]}. {LastName}";
    public string FullNameInverted => $"{LastName}, {FirstName}";

    private ApplicationUser() { }

    public ApplicationUser(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate)
    {
        Id = Guid.NewGuid();
        
        Email = email;
        UserName = email;
        
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ModifiedAtUtc = DateTimeOffset.UtcNow;

        IsActive = true;
    }
    
    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
    public void UpdateBirthDate(DateOnly birthDate)
    {
        BirthDate = birthDate;
        ModifiedAtUtc = DateTimeOffset.UtcNow;   
    }
    
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
    
    public void Activate()
    {
        if(IsActive) return;
        
        IsActive = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow;   
    }
}