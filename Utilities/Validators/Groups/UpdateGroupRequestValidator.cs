using FluentValidation;
using Utilities.Helpers;
using Utilities.Models.Requests.Groups;
using static Utilities.Helpers.ValidationHelpers;
using System.Globalization;

namespace Utilities.Validators.Groups;

public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => name == name.Trim()).WithMessage("Name cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Name must not exceed 30 characters.")
            .Matches(RegexConstants.AlphanumericPlus).WithMessage("Name contains invalid characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");

        RuleFor(x => x.Icon)
            .MaximumLength(32).WithMessage("Icon must not exceed 32 characters.")
            .Must(BeASingleGrapheme).WithMessage("Icon must be a single emoji.")
            .When(x => x.Icon != null);

        RuleFor(x => x.Active)
            .NotNull().WithMessage("Active status is required.");
    }

    private static bool BeASingleGrapheme(string? icon)
    {
        if (string.IsNullOrEmpty(icon))
        {
            return true;
        }

        var enumerator = StringInfo.GetTextElementEnumerator(icon);
        return enumerator.MoveNext() && !enumerator.MoveNext();
    }
}
