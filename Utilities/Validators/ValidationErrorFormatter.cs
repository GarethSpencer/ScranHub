using FluentValidation.Results;

namespace Utilities.Validators;

public static class ValidationErrorFormatter
{
    public static Dictionary<string, string[]> FormatErrors(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return errors;
    }
}
