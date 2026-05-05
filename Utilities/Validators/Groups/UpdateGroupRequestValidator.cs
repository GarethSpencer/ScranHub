using FluentValidation;
using Utilities.Models.Requests.Groups;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.Groups;

public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => name == name.Trim()).WithMessage("Name cannot have leading or trailing spaces.")
            .MaximumLength(30).WithMessage("Name must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");

        RuleFor(x => x.Active)
            .NotNull().WithMessage("Active status is required.");
    }
}
