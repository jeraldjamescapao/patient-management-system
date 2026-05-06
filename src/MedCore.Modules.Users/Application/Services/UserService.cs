namespace MedCore.Modules.Users.Application.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MedCore.Common.Caching;
using MedCore.Common.Localization;
using MedCore.Common.Results;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Users.Application.Abstractions;
using MedCore.Modules.Users.Application.Contracts;
using MedCore.Modules.Users.Application.Logging;

internal sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserCultureCache _userCultureCache;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        UserManager<ApplicationUser> userManager, 
        IUserCultureCache userCultureCache,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _userCultureCache = userCultureCache;
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

        return Result<UserResponse>.Success(MapToResponse(user));
    }
    
    public async Task<Result<bool>> UpdateCultureAsync(
        Guid userId, string culture, CancellationToken ct = default)
    {
        if (!SupportedCultures.All.Contains(culture))
        {
            UserLogMessages.UpdateCultureUnsupported(_logger, userId, culture, null);
            return Result<bool>.Validation(UserErrors.UnsupportedCulture);
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            UserLogMessages.UpdateCultureUserNotFound(_logger, userId, null);
            return Result<bool>.NotFound(UserErrors.UserNotFound);
        }

        user.UpdatePreferredCulture(culture, userId.ToString());
        await _userManager.UpdateAsync(user);

        _userCultureCache.InvalidateForUser(userId);

        UserLogMessages.UpdateCultureSucceeded(_logger, userId, culture, null);

        return Result<bool>.Success(true);
    }

    private static UserResponse MapToResponse(ApplicationUser user)
    {
        return new UserResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.BirthDate,
            user.PreferredCulture,
            user.IsActive,
            user.CreatedAtUtc,
            user.ModifiedAtUtc);
    }
        
}