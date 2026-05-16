using FluentValidation;
using Utilities.Helpers;
using Utilities.Models.Requests.GroupVenues;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.GroupVenues;

public class UpdateGroupVenueRequestValidator : AbstractValidator<UpdateGroupVenueRequest>
{
    public UpdateGroupVenueRequestValidator()
    {
        RuleFor(x => x.VenueName)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => name == name.Trim()).WithMessage("Name cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.")
            .Matches(RegexConstants.AlphanumericPlus).WithMessage("Name contains invalid characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");

        RuleFor(x => x.Visited)
            .NotNull().WithMessage("Visited status is required.");
    }
}
