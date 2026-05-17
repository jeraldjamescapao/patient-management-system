namespace MedCorVis.Modules.CodeItems.Application.Contracts.Requests;

using System.ComponentModel.DataAnnotations;
using MedCorVis.Modules.CodeItems.Domain;
using MedCorVis.Common.Validations;

[ValidDateRange(nameof(ValidFrom), nameof(ValidTo))]
public sealed record UpdateItemRequest(
    [Required] [Range(1, int.MaxValue)] int SortOrder,
    [MaxLength(Category.DescriptionMaxLength)] string? Description = null,
    DateOnly? ValidFrom = null,
    DateOnly? ValidTo   = null);