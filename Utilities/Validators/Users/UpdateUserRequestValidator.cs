using FluentValidation;
using Utilities.Helpers;
using Utilities.Models.Requests.Users;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .Must(name => name == name.Trim()).WithMessage("Display name cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Display name must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Display name must not exceed 30 characters.")
            .Matches(RegexConstants.AlphanumericPlus).WithMessage("Name contains invalid characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Display name must not contain profanity.");

        RuleFor(x => x.Admin)
            .NotNull().WithMessage("Admin status is required.");

        RuleFor(x => x.Active)
            .NotNull().WithMessage("Active status is required.");
    }
}
