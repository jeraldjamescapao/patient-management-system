namespace MedCore.Common.Validations;

using System.ComponentModel.DataAnnotations;

public sealed class PastDateAttribute : ValidationAttribute
{
    public PastDateAttribute()
    {
        ErrorMessage = "Date must be in the past.";
    }

    public override bool IsValid(object? value)
    {
        return value is DateOnly date && date < DateOnly.FromDateTime(DateTime.UtcNow);
    }
}