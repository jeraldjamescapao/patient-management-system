namespace MedCorVis.Common.Validations;

using System.ComponentModel.DataAnnotations;

public sealed class ValidDateRangeAttribute : ValidationAttribute
{
    private readonly string _fromProperty;
    private readonly string _toProperty;
    
    public ValidDateRangeAttribute(string fromProperty, string toProperty)
    {
        _fromProperty = fromProperty;
        _toProperty = toProperty;   
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var from = ctx.ObjectType.GetProperty(_fromProperty)?.GetValue(ctx.ObjectInstance) as DateOnly?;
        var to   = ctx.ObjectType.GetProperty(_toProperty)?.GetValue(ctx.ObjectInstance)   as DateOnly?;

        if (from.HasValue && to.HasValue && from.Value >= to.Value)
            return new ValidationResult(
                $"{_fromProperty} must be before {_toProperty}.",
                [_fromProperty, _toProperty]);

        return ValidationResult.Success;
    }
}