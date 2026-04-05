namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using MedCore.Common.Validations;
using System.ComponentModel.DataAnnotations;

public sealed record RegisterRequest(
    [Required] [MaxLength(100)] string FirstName,
    [Required] [MaxLength(100)] string LastName,
    [Required] [EmailAddress] [MaxLength(256)] string Email,
    [Required] [MinLength(8)] [MaxLength(100)] string Password,
    [PastDate] DateOnly BirthDate);