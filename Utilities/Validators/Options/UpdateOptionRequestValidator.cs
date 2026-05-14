using FluentValidation;
using Utilities.Models.Requests.Options;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.Options;

public class UpdateOptionRequestValidator : AbstractValidator<UpdateOptionRequest>
{
    public UpdateOptionRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required.")
            .Must(name => name == name.Trim()).WithMessage("Label cannot have leading or trailing spaces.")
            .MaximumLength(30).WithMessage("Label must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Label must not contain profanity.");
    }
}
