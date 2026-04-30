using FluentValidation;
using Utilities.Models.Requests;

namespace Utilities.Validators.Group;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(30).WithMessage("Name must not exceed 30 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");
    }

    private static bool CheckProfanity(string groupName)
    {
        var filter = new ProfanityFilter.ProfanityFilter();
        var detected = filter.DetectAllProfanities(groupName);
        return detected == null || detected.Count == 0;
    }
}
