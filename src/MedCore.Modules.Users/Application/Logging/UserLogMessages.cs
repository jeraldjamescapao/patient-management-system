namespace MedCore.Modules.Users.Application.Logging;

using Microsoft.Extensions.Logging;

public static class UserLogMessages
{
    #region GetCurrentUser
    
    public static readonly Action<ILogger, Guid, Exception?> GetCurrentUserNotFound =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(3001, "GetCurrentUserNotFound"),
            "Get current user failed: user {UserId} not found.");

    public static readonly Action<ILogger, Guid, Exception?> GetCurrentUserSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(3002, "GetCurrentUserSucceeded"),
            "Current user profile retrieved for user {UserId}.");
    
    #endregion
    
    #region Update Culture

    public static readonly Action<ILogger, Guid, string, Exception?> UpdateCultureUnsupported =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(3003, "UpdateCultureUnsupported"),
            "Culture update rejected for user {UserId}: '{Culture}' is not supported.");

    public static readonly Action<ILogger, Guid, Exception?> UpdateCultureUserNotFound =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(3004, "UpdateCultureUserNotFound"),
            "Culture update failed: user {UserId} not found.");

    public static readonly Action<ILogger, Guid, string, Exception?> UpdateCultureSucceeded =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(3005, "UpdateCultureSucceeded"),
            "User {UserId} updated preferred culture to '{Culture}'.");

    #endregion
}