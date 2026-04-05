namespace MedCore.Modules.Identity.Domain.Users;

using Microsoft.AspNetCore.Identity;
using MedCore.Common.Auditing;

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public const string SelfRegisteredActor = "Self";
    
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public string ModifiedBy { get; private set; } = null!;

    public string FullName => $"{FirstName} {LastName}";
    public string FullNameInverted => $"{LastName}, {FirstName}";
    
    public string FullNameWithInitials =>
        !string.IsNullOrEmpty(FirstName)
            ? $"{FirstName[..1]}. {LastName}"
            : LastName;

    private ApplicationUser() { }

    private ApplicationUser(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate,
        string createdBy)
    {
        Id = Guid.NewGuid();
        
        Email = email;
        UserName = email;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        ModifiedBy = createdBy;
        IsActive = true;
    }

    public static ApplicationUser Create(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate,
        string createdBy)
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
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy is required.");
        
        return new ApplicationUser(
            email.Trim(), 
            firstName.Trim(), 
            lastName.Trim(), 
            birthDate,
            createdBy);
    }
    
    public void UpdateName(string firstName, string lastName, string modifiedBy)
    {
        FirstName = firstName;
        LastName = lastName;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    public void UpdateBirthDate(DateOnly birthDate, string modifiedBy)
    {
        BirthDate = birthDate;
        ModifiedAtUtc = DateTimeOffset.UtcNow;   
        ModifiedBy = modifiedBy;
    }
    
    public void Deactivate(string modifiedBy)
    {
        if (!IsActive) return;
        
        IsActive = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    public void Activate(string modifiedBy)
    {
        if(IsActive) return;
        
        IsActive = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow; 
        ModifiedBy = modifiedBy;
    }
}