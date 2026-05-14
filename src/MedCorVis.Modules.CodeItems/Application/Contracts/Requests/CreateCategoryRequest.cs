namespace MedCorVis.Modules.CodeItems.Application.Contracts.Requests;

using MedCorVis.Modules.CodeItems.Domain;
using System.ComponentModel.DataAnnotations;

public sealed record CreateCategoryRequest(
    [Required] [MaxLength(Category.CodeMaxLength)] string Code,
    [Required] [Range(1, int.MaxValue)] int SortOrder,
    [MaxLength(Category.DescriptionMaxLength)] string? Description = null);