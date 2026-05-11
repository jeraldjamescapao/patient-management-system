namespace MedCore.Modules.Localization.Domain;

using MedCore.Common.Auditing;

internal sealed class Translation : IAuditableEntity
{
    public long Id { get; private set; }
    public string Culture { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    private Translation() { }

    public Translation(string culture, string key, string value, string createdBy, string? description = null)
    {
        Culture = culture;
        Key = key;
        Value = value;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }
}