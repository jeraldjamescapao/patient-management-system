namespace MedCore.Modules.Users.Application.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Users.Application.Abstractions;
using MedCore.Modules.Users.Application.Contracts;
using MedCore.Modules.Users.Application.Logging;

internal sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;
    
    public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    
    public async Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            UserLogMessages.GetCurrentUserNotFound(_logger, userId, null);
            return Result<UserResponse>.NotFound(UserErrors.UserNotFound);
        }

        UserLogMessages.GetCurrentUserSucceeded(_logger, userId, null);

        return Result<UserResponse>.Success(new UserResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.BirthDate,
            user.PreferredCulture,
            user.IsActive,
            user.CreatedAtUtc,
            user.ModifiedAtUtc));
    }
}