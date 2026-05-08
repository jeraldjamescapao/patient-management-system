namespace MedCore.Modules.Users.Application.Contracts;

using System.ComponentModel.DataAnnotations;

public sealed record UpdatePhoneRequest(
    [Phone] [MaxLength(20)] string? PhoneNumber);