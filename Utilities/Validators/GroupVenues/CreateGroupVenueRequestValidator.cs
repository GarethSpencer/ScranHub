using FluentValidation;
using Utilities.Models.Requests.GroupVenues;
using static Utilities.Helpers.ValidationHelpers;

namespace Utilities.Validators.GroupVenues;

public class CreateGroupVenueRequestValidator : AbstractValidator<CreateGroupVenueRequest>
{
    public CreateGroupVenueRequestValidator()
    {
        RuleFor(x => x.VenueName)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => name == name.Trim()).WithMessage("Name cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");
    }
}
