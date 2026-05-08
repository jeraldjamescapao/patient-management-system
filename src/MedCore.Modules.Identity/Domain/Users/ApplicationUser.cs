namespace MedCore.Modules.Identity.Domain.Users;

using MedCore.Common.Exceptions;
using MedCore.Common.Localization;
using Microsoft.AspNetCore.Identity;
using MedCore.Common.Auditing;

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public const string SelfRegisteredActor = "Self";
    
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string? PreferredCulture { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public string ModifiedBy { get; private set; } = null!;

    public string FullName => $"{FirstName} {LastName}";
    public string FullNameInverted => $"{LastName}, {FirstName}";
    
    public string FullNameWithInitials => 
        FirstName.Length > 0 ? $"{FirstName[0]}. {LastName}" : FullName;

    private ApplicationUser() { }

    private ApplicationUser(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate,
        string createdBy,
        string? preferredCulture)
    {
        Id = Guid.NewGuid();
        
        Email = email;
        UserName = email;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        PreferredCulture = preferredCulture;
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
        string createdBy,
        string? preferredCulture = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("DOMAIN_USER_INVALID_EMAIL", "Email is required.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("DOMAIN_USER_INVALID_FIRST_NAME", "FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("DOMAIN_USER_INVALID_LAST_NAME", "LastName is required.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("DOMAIN_USER_INVALID_BIRTH_DATE", "BirthDate cannot be in the future.");
        if (preferredCulture is not null &&
            !SupportedCultures.All.Contains(preferredCulture))
            throw new DomainException("DOMAIN_USER_INVALID_CULTURE", "Unsupported culture.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("DOMAIN_USER_INVALID_CREATED_BY", "CreatedBy is required.");
        
        return new ApplicationUser(
            email.Trim(), 
            firstName.Trim(), 
            lastName.Trim(), 
            birthDate,
            createdBy,
            preferredCulture);
    }
    
    public void UpdateName(string firstName, string lastName, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("DOMAIN_USER_INVALID_FIRST_NAME", "FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("DOMAIN_USER_INVALID_LAST_NAME", "LastName is required.");
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.");       
        
        if (firstName == FirstName && lastName == LastName) return;
        
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    public void UpdateBirthDate(DateOnly birthDate, string modifiedBy)
    {
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("DOMAIN_USER_INVALID_BIRTH_DATE", "BirthDate cannot be in the future.");
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required."); 
        
        if (birthDate == BirthDate) return;
        
        BirthDate = birthDate;
        ModifiedAtUtc = DateTimeOffset.UtcNow;   
        ModifiedBy = modifiedBy;
    }

    public void UpdateProfile(
        string firstName, 
        string lastName, 
        DateOnly birthDate, 
        string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("DOMAIN_USER_INVALID_FIRST_NAME", "FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("DOMAIN_USER_INVALID_LAST_NAME", "LastName is required.");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("DOMAIN_USER_INVALID_BIRTH_DATE", "BirthDate cannot be in the future.");
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.");
        
        var nameChanged = firstName.Trim() != FirstName || lastName.Trim() != LastName;
        var birthDateChanged = birthDate != BirthDate;
        
        if (!nameChanged && !birthDateChanged) return;
        
        if (nameChanged)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        if (birthDateChanged)
            BirthDate = birthDate;
        
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    public void UpdatePreferredCulture(string culture, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(culture))
            throw new DomainException("DOMAIN_USER_INVALID_CULTURE", "Culture is required.");
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required."); 
        
        if (culture == PreferredCulture) return;

        PreferredCulture = culture;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    public void Deactivate(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required."); 
        
        if (!IsActive) return;
        
        IsActive = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    public void Activate(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required."); 
        
        if (IsActive) return;
        
        IsActive = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow; 
        ModifiedBy = modifiedBy;
    }
}