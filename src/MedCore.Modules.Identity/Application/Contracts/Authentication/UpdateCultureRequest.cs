namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using System.ComponentModel.DataAnnotations;

public sealed record UpdateCultureRequest(
    [Required] [MaxLength(10)] string Culture);