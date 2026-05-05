namespace MedCore.Modules.Users.Application.Abstractions;

using MedCore.Common.Results;
using MedCore.Modules.Users.Application.Contracts;

public interface IUserService
{
    Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}