using FluentValidation;
using Utilities.Models.Requests.Groups;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.Groups;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => name == name.Trim()).WithMessage("Name cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Name must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");
    }
}
