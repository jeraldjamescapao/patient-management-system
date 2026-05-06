namespace MedCore.Modules.Users.Application.Contracts;

using System.ComponentModel.DataAnnotations;

public sealed record UpdateCultureRequest(
    [Required] [MaxLength(10)] string Culture);