using FluentValidation;
using Utilities.Models.Requests.Users;

namespace Utilities.Validators.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .Must(name => name == name.Trim()).WithMessage("Display name cannot have leading or trailing spaces.")
            .MaximumLength(30).WithMessage("Display name must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Display name must not contain profanity.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(email => email == email.Trim()).WithMessage("Email cannot have leading or trailing spaces.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Admin)
            .NotNull().WithMessage("Admin status is required.");
    }

    private static bool CheckProfanity(string groupName)
    {
        var filter = new ProfanityFilter.ProfanityFilter();
        var detected = filter.DetectAllProfanities(groupName);
        return detected == null || detected.Count == 0;
    }
}
