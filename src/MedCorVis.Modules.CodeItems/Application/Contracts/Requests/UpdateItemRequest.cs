namespace MedCorVis.Modules.CodeItems.Application.Contracts.Requests;

using System.ComponentModel.DataAnnotations;
using MedCorVis.Modules.CodeItems.Domain;

public sealed record UpdateItemRequest(
    [Required] [Range(1, int.MaxValue)] int SortOrder,
    [MaxLength(Category.DescriptionMaxLength)] string? Description = null);