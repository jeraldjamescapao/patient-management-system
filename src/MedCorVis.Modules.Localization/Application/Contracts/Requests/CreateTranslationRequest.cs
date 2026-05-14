namespace MedCorVis.Modules.Localization.Application.Contracts.Requests;

using MedCorVis.Modules.Localization.Domain;
using System.ComponentModel.DataAnnotations;
    
public sealed record CreateTranslationRequest(
    [Required] [MaxLength(Translation.CultureMaxLength)] string Culture,
    [Required] [MaxLength(Translation.KeyMaxLength)] string Key,
    [Required] string Value,
    [MaxLength(Translation.DescriptionMaxLength)] string? Description);