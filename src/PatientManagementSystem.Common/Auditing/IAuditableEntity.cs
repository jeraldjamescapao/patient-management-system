namespace PatientManagementSystem.Common.Auditing;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }
    DateTimeOffset ModifiedAtUtc { get; }
    string CreatedBy { get; }
    string ModifiedBy { get; }
}