namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using System.Text.Json.Serialization;

public sealed record RefreshResponse(string AccessToken)
{
    [JsonIgnore]
    public string RawRefreshToken { get; init; } = string.Empty;
};