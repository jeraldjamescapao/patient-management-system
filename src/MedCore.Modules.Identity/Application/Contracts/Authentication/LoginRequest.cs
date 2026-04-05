namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using MedCore.Common.Validations;
using System.ComponentModel.DataAnnotations;

public sealed record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password);