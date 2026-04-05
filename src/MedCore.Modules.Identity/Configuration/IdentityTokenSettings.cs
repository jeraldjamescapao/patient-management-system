namespace MedCore.Modules.Identity.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class IdentityTokenSettings
{
    public const string SectionName = "IdentityTokens";
    
    /// <summary>Use <see cref="NormalizedEmailConfirmationPath"/> when building email confirmation paths.</summary>
    [Required] public string EmailConfirmationPath { get; init; } = null!;
    
    public string NormalizedEmailConfirmationPath => "/" + EmailConfirmationPath.TrimStart('/');
    
    [Range(1, int.MaxValue)] public int EmailConfirmationExpirationInHours { get; init; }
}