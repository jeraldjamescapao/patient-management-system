namespace MedCore.Modules.Users.Application.Contracts;

using MedCore.Common.Results;

public static class UserErrors
{
    public static readonly ResultError UserNotFound =
        new("USERS_USER_NOT_FOUND", "User not found.");
    
    public static readonly ResultError InvalidToken =
        new("USERS_INVALID_TOKEN", "Invalid or missing authentication token.");
    
    public static readonly ResultError UnsupportedCulture =
        new("USERS_UNSUPPORTED_CULTURE", "The specified culture is not supported.");
    
    public static readonly ResultError ProfileUpdateFailed =
        new("USERS_UPDATE_FAILED", "Failed to update user profile.");

    public static readonly ResultError PhoneUpdateFailed =
        new("USERS_PHONE_UPDATE_FAILED", "Failed to update phone number.");
}