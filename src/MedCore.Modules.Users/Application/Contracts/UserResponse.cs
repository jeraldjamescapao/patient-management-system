namespace MedCore.Modules.Users.Application.Contracts;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    DateOnly BirthDate,
    string? Culture,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ModifiedAtUtc);