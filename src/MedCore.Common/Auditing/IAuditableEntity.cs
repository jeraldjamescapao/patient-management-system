namespace MedCore.Common.Auditing;

using System;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }
    string CreatedBy { get; }
    DateTimeOffset? ModifiedAtUtc { get; }
    string? ModifiedBy { get; }
}