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

    public static ApplicationUser Create(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate)
    {
        // The guards are here to ensure validity!
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("BirthDate cannot be in the future!");
        
        return new ApplicationUser(
            email.Trim(), 
            firstName.Trim(), 
            lastName.Trim(), 
            birthDate);
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