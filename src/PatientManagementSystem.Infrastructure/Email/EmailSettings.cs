namespace PatientManagementSystem.Infrastructure.Email;

using System.ComponentModel.DataAnnotations;

public sealed class EmailSettings
{
    public const string SectionName = "Email";
    
    [Required] public string Host { get; init; } = null!;
    [Range(1, 65535)] public int Port { get; init; }
    [Required] public string Username { get; init; } = null!;
    [Required] public string Password { get; init; } = null!;
    [Required] [EmailAddress] public string FromAddress { get; init; } = null!;
    [Required] public string FromName { get; init; } = null!;
    
    public string SecureSocket { get; init; } = "StartTls"; // default
}