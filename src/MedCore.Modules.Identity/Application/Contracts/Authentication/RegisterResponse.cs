namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using System.Text.Json.Serialization;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FullName,
    IList<string> Roles,
    string AccessToken)
{
    [JsonIgnore]
    public string RawRefreshToken { get; init; } = string.Empty;
};