namespace MedCorVis.Modules.CodeItems.Application.Contracts.Requests;

using System.ComponentModel.DataAnnotations;
using MedCorVis.Modules.CodeItems.Domain;

public sealed record UpsertTranslationRequest(
    [Required] [MaxLength(CodeItemTranslation.LabelMaxLength)] string Label,
    [MaxLength(Category.DescriptionMaxLength)] string? Description = null);