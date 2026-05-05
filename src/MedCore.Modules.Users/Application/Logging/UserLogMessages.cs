namespace MedCore.Modules.Users.Application.Logging;

using Microsoft.Extensions.Logging;

public static class UserLogMessages
{
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
}