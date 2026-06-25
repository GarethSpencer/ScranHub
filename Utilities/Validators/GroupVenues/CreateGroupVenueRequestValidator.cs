using FluentValidation;
using Utilities.Helpers;
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
            .Matches(RegexConstants.AlphanumericPlus).WithMessage("Name contains invalid characters.")
            .Must(name => CheckProfanity(name)).WithMessage("Name must not contain profanity.");

        RuleFor(x => x.GooglePlaceId)
            .MaximumLength(255).WithMessage("GooglePlaceId must not exceed 255 characters.")
            .Matches(RegexConstants.GooglePlaceId).WithMessage("GooglePlaceId contains invalid characters.")
            .When(x => x.GooglePlaceId != null);

        RuleFor(x => x.FormattedAddress)
            .MaximumLength(512).WithMessage("FormattedAddress must not exceed 512 characters.")
            .Matches(RegexConstants.FormattedAddress).WithMessage("GooglePlaceId contains invalid characters.")
            .When(x => x.FormattedAddress != null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude.HasValue);
    }
}
