namespace MedCore.Modules.Identity.Application.Contracts.Authentication;

using System.ComponentModel.DataAnnotations;

public sealed record ResendConfirmationEmailRequest(
    [Required] [EmailAddress] string Email);