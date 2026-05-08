namespace MedCore.Modules.Users.Application.Abstractions;

using MedCore.Common.Results;
using MedCore.Modules.Users.Application.Contracts;

public interface IUserService
{
    Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> UpdateCultureAsync(Guid userId, string culture, CancellationToken ct = default);
    Task<Result<UserResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<Result<bool>> UpdatePhoneAsync(Guid userId, string? phoneNumber, CancellationToken ct = default);
}