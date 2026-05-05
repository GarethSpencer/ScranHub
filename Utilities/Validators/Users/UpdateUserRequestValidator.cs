using FluentValidation;
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
            .MaximumLength(30).WithMessage("Display name must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Display name must not contain profanity.");

        RuleFor(x => x.Admin)
            .NotNull().WithMessage("Admin status is required.");

        RuleFor(x => x.Active)
            .NotNull().WithMessage("Active status is required.");
    }
}
