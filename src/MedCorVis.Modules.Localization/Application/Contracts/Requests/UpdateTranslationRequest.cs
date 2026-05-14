namespace MedCorVis.Modules.Localization.Application.Contracts.Requests;

using MedCorVis.Modules.Localization.Domain;
using System.ComponentModel.DataAnnotations;

public sealed record UpdateTranslationRequest(
    [Required] string Value,
    [MaxLength(Translation.DescriptionMaxLength)] string? Description);