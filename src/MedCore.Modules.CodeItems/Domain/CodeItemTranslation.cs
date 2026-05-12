namespace MedCore.Modules.CodeItems.Domain;

using MedCore.Common.Auditing;
using MedCore.Common.Exceptions;
using MedCore.Common.Localization;

internal sealed class CodeItemTranslation : IAuditableEntity
{
    public const string EntityTypeCategory  = "Category";
    public const string EntityTypeItem      = "Item";
    public const int    EntityTypeMaxLength = 20;
    public const int    CultureMaxLength    = 10;
    public const int    LabelMaxLength      = 200;
    public const int    DescriptionMaxLength = 500;

    public long             Id            { get; private set; }
    public string           EntityType    { get; private set; } = null!;
    public long             EntityId      { get; private set; }
    public string           Culture       { get; private set; } = null!;
    public string           Label         { get; private set; } = null!;
    public string?          Description   { get; private set; }

    // Business Control
    public bool             IsSystemDefined { get; private set; }

    // Visibility
    public bool             IsActive      { get; private set; }

    // Soft Delete
    public bool             IsDeleted     { get; private set; }
    public DateTimeOffset?  DeletedAtUtc  { get; private set; }
    public string?          DeletedBy     { get; private set; }

    // Audit
    public DateTimeOffset   CreatedAtUtc  { get; private set; }
    public string           CreatedBy     { get; private set; } = null!;
    public DateTimeOffset?  ModifiedAtUtc { get; private set; }
    public string?          ModifiedBy    { get; private set; }

    private CodeItemTranslation() { }

    private CodeItemTranslation(
        string entityType,
        long entityId,
        string culture,
        string label,
        string? description,
        bool isSystemDefined,
        string createdBy)
    {
        EntityType      = entityType;
        EntityId        = entityId;
        Culture         = culture;
        Label           = label;
        Description     = description;
        IsSystemDefined = isSystemDefined;
        IsActive        = true;
        IsDeleted       = false;
        CreatedAtUtc    = DateTimeOffset.UtcNow;
        CreatedBy       = createdBy;
    }

    public static CodeItemTranslation Create(
        string entityType,
        long entityId,
        string culture,
        string label,
        string? description,
        bool isSystemDefined,
        string createdBy)
    {
        if (entityType != EntityTypeCategory && entityType != EntityTypeItem)
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_ENTITY_TYPE",
                "EntityType must be '{EntityTypeCategory}' or '{EntityTypeItem}'.");
        
        if (entityId <= 0)
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_ENTITY_ID",
                "EntityId must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(culture))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_CULTURE",
                "Culture is required.");
        
        var trimmedCulture = culture.Trim();
        
        if (!SupportedCultures.All.Contains(trimmedCulture))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_CULTURE",
                "Unsupported culture.");
        
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_LABEL",
                "Label is required.");
        
        var trimmedLabel = label.Trim();
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_CREATED_BY",
                "CreatedBy is required.");

        return new CodeItemTranslation(
            entityType,
            entityId,
            trimmedCulture,
            trimmedLabel,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            isSystemDefined,
            createdBy);
    }

    public void Update(string label, string? description, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_LABEL",
                "Label is required.");
        
        var trimmedLabel = label.Trim();
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_MODIFIED_BY",
                "ModifiedBy is required.");

        Label         = trimmedLabel;
        Description   = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive      = true;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_MODIFIED_BY",
                "ModifiedBy is required.");

        if (!IsActive) return;

        IsActive      = false;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy    = modifiedBy;
    }

    public void Delete(string deletedBy)
    {
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new DomainException(
                "DOMAIN_CODEITEMTRANSLATION_INVALID_DELETED_BY",
                "DeletedBy is required.");

        if (IsDeleted) return;

        IsDeleted    = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        DeletedBy    = deletedBy;
    }
}