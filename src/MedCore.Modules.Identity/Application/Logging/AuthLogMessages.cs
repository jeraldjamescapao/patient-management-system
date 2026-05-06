namespace MedCore.Modules.Identity.Application.Logging;

using Microsoft.Extensions.Logging;

public static class AuthLogMessages
{
    #region Register
    
    public static readonly Action<ILogger, string, Exception?> RegisterEmailConflict =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2001, "RegisterEmailConflict"),
            "Registration rejected: email {Email} is already registered.");
    
    public static readonly Action<ILogger, string, Exception?> RegisterFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2002, "RegisterFailed"),
            "Registration failed for email {Email}: identity creation returned errors.");
 
    public static readonly Action<ILogger, string, Exception?> RegisterRoleAssignmentFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2003, "RegisterRoleAssignmentFailed"),
            "Registration failed for email {Email}: role assignment returned errors.");
 
    public static readonly Action<ILogger, string, Exception?> RegisterEmailDeliveryFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2004, "RegisterEmailDeliveryFailed"),
            "Registration rolled back for email {Email}: confirmation email delivery failed.");
 
    public static readonly Action<ILogger, Guid, string, Exception?> RegisterSucceeded =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2005, "RegisterSucceeded"),
            "User {UserId} registered successfully with email {Email}.");
    
    #endregion
    
    #region Login
    
    public static readonly Action<ILogger, string, Exception?> LoginUserNotFound =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2010, "LoginUserNotFound"),
            "Login failed: no account found for email {Email}.");
 
    public static readonly Action<ILogger, Guid, Exception?> LoginAccountDeactivated =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2011, "LoginAccountDeactivated"),
            "Login rejected: account {UserId} is deactivated.");
 
    public static readonly Action<ILogger, Guid, Exception?> LoginEmailNotConfirmed =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2012, "LoginEmailNotConfirmed"),
            "Login rejected: email not confirmed for account {UserId}.");
 
    public static readonly Action<ILogger, Guid, Exception?> LoginInvalidPassword =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2013, "LoginInvalidPassword"),
            "Login failed: invalid password for account {UserId}.");
 
    public static readonly Action<ILogger, Guid, string, Exception?> LoginSucceeded =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2014, "LoginSucceeded"),
            "User {UserId} logged in successfully with email {Email}.");
    
    #endregion
    
    #region Refresh
    
    public static readonly Action<ILogger, Exception?> RefreshTokenMissing =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2020, "RefreshTokenMissing"),
            "Token refresh rejected: no refresh token provided.");
 
    public static readonly Action<ILogger, Exception?> RefreshTokenNotFound =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2021, "RefreshTokenNotFound"),
            "Token refresh rejected: refresh token not found in store.");
 
    public static readonly Action<ILogger, Guid, Exception?> RefreshTokenReuseDetected =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2022, "RefreshTokenReuseDetected"),
            "Token reuse detected for user {UserId}: entire token family revoked.");
 
    public static readonly Action<ILogger, Exception?> RefreshTokenExpiredOrRevoked =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2023, "RefreshTokenExpiredOrRevoked"),
            "Token refresh rejected: token is expired or revoked.");
 
    public static readonly Action<ILogger, Guid, Exception?> RefreshTokenUserInvalid =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2024, "RefreshTokenUserInvalid"),
            "Token refresh rejected: user {UserId} not found or deactivated.");
 
    public static readonly Action<ILogger, Guid, Exception?> RefreshSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(2025, "RefreshSucceeded"),
            "Token pair refreshed successfully for user {UserId}.");
    
    #endregion
    
    #region Logout
    
    public static readonly Action<ILogger, Guid, Exception?> LogoutSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(2030, "LogoutSucceeded"),
            "User {UserId} logged out: refresh token revoked.");
 
    public static readonly Action<ILogger, Exception?> LogoutTokenMissingOrInactive =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(2031, "LogoutTokenMissingOrInactive"),
            "Logout called with no active token; treated as success.");
 
    public static readonly Action<ILogger, Guid, Exception?> LogoutAllSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(2032, "LogoutAllSucceeded"),
            "All sessions revoked for user {UserId}.");
    
    #endregion
    
    #region Email Confirmation

    public static readonly Action<ILogger, Guid, Exception?> ConfirmEmailUserNotFound =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2040, "ConfirmEmailUserNotFound"),
            "Email confirmation failed: user {UserId} not found.");
 
    public static readonly Action<ILogger, Guid, Exception?> ConfirmEmailAlreadyConfirmed =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2041, "ConfirmEmailAlreadyConfirmed"),
            "Email confirmation skipped: user {UserId} email is already confirmed.");
 
    public static readonly Action<ILogger, Guid, Exception?> ConfirmEmailInvalidToken =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(2042, "ConfirmEmailInvalidToken"),
            "Email confirmation failed for user {UserId}: invalid or expired token.");
 
    public static readonly Action<ILogger, Guid, Exception?> ConfirmEmailSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(2043, "ConfirmEmailSucceeded"),
            "Email confirmed successfully for user {UserId}.");
    
    #endregion

    #region Resend Confirmation Email
    
    public static readonly Action<ILogger, Exception?> ResendConfirmationEmailDeliveryFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2050, "ResendConfirmationEmailDeliveryFailed"),
            "Resend confirmation email delivery failed.");
 
    public static readonly Action<ILogger, Guid, Exception?> ResendConfirmationSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(2051, "ResendConfirmationSucceeded"),
            "Confirmation email resent successfully for user {UserId}.");
    
    #endregion
}