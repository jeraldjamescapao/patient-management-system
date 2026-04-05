namespace MedCore.Modules.Identity.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required] public string SecretKey { get; init; } = null!;
    [Required] public string Issuer { get; init; } = null!;
    [Required] public string Audience { get; init; } = null!;
    [Range(1, int.MaxValue)] public int AccessTokenExpirationInMinutes { get; init; }
    [Range(1, int.MaxValue)] public int RefreshTokenExpirationInDays { get; init; }
}