namespace MedCorVis.Modules.Identity.Domain.Users;

using MedCorVis.Common.Domain;
using Microsoft.AspNetCore.Identity;
using MedCorVis.Common.Auditing;

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, IDeletableEntity
{
    #region Constants
    
    public const string SelfRegisteredActor = "Self";
    public const int FirstNameMinLength = 2;
    public const int FirstNameMaxLength = 100;
    public const int LastNameMinLength = 2;
    public const int LastNameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PreferredCultureMaxLength = 10;    
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    
    #endregion
    
    #region Properties
    
    // User Profile
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string? PreferredCulture { get; private set; }
    
    // Visibility
    public bool IsActive { get; private set; } = true;
    
    // Soft Delete
    public bool             IsDeleted     { get; private set; }
    public DateTimeOffset?  DeletedAtUtc  { get; private set; }
    public string?          DeletedBy     { get; private set; }
    public DateTimeOffset?  DeletionRequestedAtUtc { get; private set; }
    
    // Audit
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ModifiedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public string? ModifiedBy { get; private set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public string FullNameInverted => $"{LastName}, {FirstName}";
    public string FullNameWithInitials =>
        FirstName.Length > 0 ? $"{FirstName[0]}. {LastName}" : LastName;

    #endregion
    
    #region Constructors
    
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
        CreatedBy = createdBy;
        IsActive = true;
    }
    
    #endregion   
    
    #region Factory

    public static ApplicationUser Create(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate,
        string createdBy,
        string? preferredCulture = null)
    {
        var trimmedEmail = DomainGuards.RequireNonEmpty(
            email, "DOMAIN_USER_INVALID_EMAIL", "Email is required.");
        var trimmedFirstName = DomainGuards.RequireNonEmpty(
            firstName, "DOMAIN_USER_INVALID_FIRST_NAME", "FirstName is required.");
        var trimmedLastName = DomainGuards.RequireNonEmpty(
            lastName, "DOMAIN_USER_INVALID_LAST_NAME", "LastName is required.");
        var trimmedCulture = DomainGuards.RequireValidCulture(
            preferredCulture, "DOMAIN_USER_INVALID_CULTURE", "Unsupported culture.");
        var trimmedCreatedBy = DomainGuards.RequireNonEmpty(
            createdBy, "DOMAIN_USER_INVALID_CREATED_BY", "CreatedBy is required.", 
            IAuditableEntity.CreatedByMaxLength);
        
        DomainGuards.RequirePastOrPresentDate(birthDate, 
            "DOMAIN_USER_INVALID_BIRTH_DATE", "BirthDate cannot be in the future.");
        
        return new ApplicationUser(
            trimmedEmail, 
            trimmedFirstName, 
            trimmedLastName, 
            birthDate,
            trimmedCreatedBy,
            trimmedCulture);
    }
    
    #endregion
    
    #region Methods

    public void UpdateProfile(
        string firstName, 
        string lastName, 
        DateOnly birthDate, 
        string modifiedBy)
    {
        var trimmedFirstName = DomainGuards.RequireNonEmpty(
            firstName, "DOMAIN_USER_INVALID_FIRST_NAME", "FirstName is required.");
        var trimmedLastName = DomainGuards.RequireNonEmpty(
            lastName, "DOMAIN_USER_INVALID_LAST_NAME", "LastName is required.");
        var trimmedModifiedBy = DomainGuards.RequireNonEmpty(
            modifiedBy, "DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.",
            IAuditableEntity.ModifiedByMaxLength);
        
        DomainGuards.RequirePastOrPresentDate(birthDate, 
            "DOMAIN_USER_INVALID_BIRTH_DATE", "BirthDate cannot be in the future.");
        
        var nameChanged = trimmedFirstName != FirstName || trimmedLastName != LastName;
        var birthDateChanged = birthDate != BirthDate;
        
        if (!nameChanged && !birthDateChanged) return;
        
        if (nameChanged)
        {
            FirstName = trimmedFirstName;
            LastName = trimmedLastName;
        }

        if (birthDateChanged)
            BirthDate = birthDate;
        
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = trimmedModifiedBy;
    }
    
    public void UpdatePreferredCulture(string culture, string modifiedBy)
    {
        var trimmedCulture = DomainGuards.RequireValidCulture(
            culture, "DOMAIN_USER_INVALID_CULTURE", "Culture is required or unsupported.");
        var trimmedModifiedBy = DomainGuards.RequireNonEmpty(
            modifiedBy, "DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.",
            IAuditableEntity.ModifiedByMaxLength);
        
        if (trimmedCulture == PreferredCulture) return;
        
        PreferredCulture = trimmedCulture;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = trimmedModifiedBy;
    }
    
    public void Deactivate(string modifiedBy)
    {
        var trimmedModifiedBy = DomainGuards.RequireNonEmpty(
            modifiedBy, "DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.",
            IAuditableEntity.ModifiedByMaxLength);
        
        if (!IsActive) return;
        
        IsActive = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = trimmedModifiedBy;
    }
    
    public void Activate(string modifiedBy)
    {
        var trimmedModifiedBy = DomainGuards.RequireNonEmpty(
            modifiedBy, "DOMAIN_USER_INVALID_MODIFIED_BY", "ModifiedBy is required.",
            IAuditableEntity.ModifiedByMaxLength);
        
        if (IsActive) return;
        
        IsActive = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow; 
        ModifiedBy = trimmedModifiedBy;
    }
    
    public void RequestDeletion()
    {
        if (IsDeleted) return;
        if (DeletionRequestedAtUtc.HasValue) return;

        DeletionRequestedAtUtc = DateTimeOffset.UtcNow;
        ModifiedAtUtc          = DateTimeOffset.UtcNow;
        ModifiedBy             = SelfRegisteredActor;
    }
    
    public void CancelDeletionRequest()
    {
        if (!DeletionRequestedAtUtc.HasValue) return;

        DeletionRequestedAtUtc = null;
        ModifiedAtUtc          = DateTimeOffset.UtcNow;
        ModifiedBy             = SelfRegisteredActor;
    }
    
    public void Delete(string deletedBy)
    {
        if (IsDeleted) return;

        var trimmedDeletedBy = DomainGuards.RequireNonEmpty(
            deletedBy, "DOMAIN_USER_INVALID_DELETED_BY", "DeletedBy is required.");

        Anonymise(trimmedDeletedBy);

        IsDeleted              = true;
        DeletedAtUtc           = DateTimeOffset.UtcNow;
        DeletedBy              = trimmedDeletedBy;
        IsActive               = false;
        DeletionRequestedAtUtc = null;
        ModifiedAtUtc          = DateTimeOffset.UtcNow;
        ModifiedBy             = trimmedDeletedBy;
    }
    
    private void Anonymise(string deletedBy)
    {
        Email               = $"deleted_{Id}@deleted.invalid";
        UserName            = $"deleted_{Id}@deleted.invalid";
        NormalizedEmail     = $"DELETED_{Id}@DELETED.INVALID";
        NormalizedUserName  = $"DELETED_{Id}@DELETED.INVALID";
        FirstName           = "Deleted";
        LastName            = "User";
        PhoneNumber         = null;
    }
    
    #endregion
}