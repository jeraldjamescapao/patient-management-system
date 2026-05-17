namespace MedCorVis.Modules.CodeItems.Domain;

using MedCorVis.Common.Auditing;
using MedCorVis.Common.Exceptions;

internal sealed class CodeItem : IAuditableEntity
{
    public const int CodeMaxLength        = 100;
    public const int DescriptionMaxLength = 500;

    public long             Id            { get; private set; }
    public long             CategoryId    { get; private set; }
    public string           Code          { get; private set; } = null!;
    public string?          Description   { get; private set; }
    public int              SortOrder     { get; private set; }
    public DateOnly? ValidFrom { get; private set; }
    public DateOnly? ValidTo   { get; private set; }

    // Visibility
    public bool             IsActive      { get; private set; }

    // Business Control
    public bool             IsSystemDefined { get; private set; }
    public bool             IsEditable      { get; private set; }
    public bool             IsDeletable     { get; private set; }

    // Soft Delete
    public bool             IsDeleted     { get; private set; }
    public DateTimeOffset?  DeletedAtUtc  { get; private set; }
    public string?          DeletedBy     { get; private set; }

    // Audit
    public DateTimeOffset   CreatedAtUtc  { get; private set; }
    public string           CreatedBy     { get; private set; } = null!;
    public DateTimeOffset?  ModifiedAtUtc { get; private set; }
    public string?          ModifiedBy    { get; private set; }

    private CodeItem() { }

    private CodeItem(
        long categoryId,
        string code,
        string? description,
        int sortOrder,
        DateOnly? validFrom,
        DateOnly? validTo,
        bool isSystemDefined,
        bool isEditable,
        bool isDeletable,
        string createdBy)
    {
        CategoryId      = categoryId;
        Code            = code;
        Description     = description;
        SortOrder       = sortOrder;
        ValidFrom       = validFrom;
        ValidTo         = validTo;
        IsActive        = true;
        IsSystemDefined = isSystemDefined;
        IsEditable      = isEditable;
        IsDeletable     = isDeletable;
        IsDeleted       = false;
        CreatedAtUtc    = DateTimeOffset.UtcNow;
        CreatedBy       = createdBy;
    }

    public static CodeItem Create(
        long categoryId,
        string code,
        string? description,
        int sortOrder,
        DateOnly? validFrom,
        DateOnly? validTo,
        bool isSystemDefined,
        bool isEditable,
        bool isDeletable,
        string createdBy)
    {
        if (categoryId <= 0)
            throw new DomainException("DOMAIN_CODEITEM_INVALID_CATEGORY", 
                "CategoryId must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_CODE", 
                "Code is required.");
        
        var trimmedCode = code.Trim();
        
        if (trimmedCode.Length > CodeMaxLength)
            throw new DomainException("DOMAIN_CODEITEM_INVALID_CODE", 
                $"Code cannot exceed {CodeMaxLength} characters.");
        
        if (validFrom.HasValue && validTo.HasValue && validFrom.Value >= validTo.Value)
            throw new DomainException("DOMAIN_CODEITEM_INVALID_VALIDITY_WINDOW", 
                "ValidFrom must be before ValidTo.");
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_CREATED_BY", 
                "CreatedBy is required.");

        return new CodeItem(
            categoryId,
            trimmedCode,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            sortOrder,
            validFrom,
            validTo,
            isSystemDefined,
            isEditable,
            isDeletable,
            createdBy);
    }

    public void Update(string? description, int sortOrder, string modifiedBy)
    {
        if (!IsEditable)
            throw new DomainException("DOMAIN_CODEITEM_NOT_EDITABLE", "This item cannot be edited.");
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_MODIFIED_BY", "ModifiedBy is required.");

        Description   = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder     = sortOrder;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }

    public void Activate(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_MODIFIED_BY", "ModifiedBy is required.");

        if (IsActive) return;

        IsActive      = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_MODIFIED_BY", "ModifiedBy is required.");

        if (!IsActive) return;

        IsActive      = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }

    public void Delete(string deletedBy)
    {
        if (!IsDeletable)
            throw new DomainException("DOMAIN_CODEITEM_NOT_DELETABLE", "This item cannot be deleted.");
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_DELETED_BY", "DeletedBy is required.");

        if (IsDeleted) return;

        IsDeleted    = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        DeletedBy    = deletedBy;
    }

    public void SetValidity(DateOnly? validFrom, DateOnly? validTo, string modifiedBy)
    {
        if (!IsEditable)
            throw new DomainException("DOMAIN_CODEITEM_NOT_EDITABLE", 
                "This item cannot be edited.");

        if (validFrom.HasValue && validTo.HasValue && validFrom.Value >= validTo.Value)
            throw new DomainException("DOMAIN_CODEITEM_INVALID_VALIDITY_WINDOW",
                "ValidFrom must be before ValidTo.");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("DOMAIN_CODEITEM_INVALID_MODIFIED_BY", 
                "ModifiedBy is required.");
        
        ValidFrom     = validFrom;
        ValidTo       = validTo;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }
}