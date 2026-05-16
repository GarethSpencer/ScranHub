using FluentValidation;
using Utilities.Helpers;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators.Generic;

namespace Utilities.Validators.GroupVenues;

public class SearchGroupVenueRequestValidator : AbstractValidator<SearchGroupVenueRequest>
{
    public SearchGroupVenueRequestValidator()
    {
        Include(new PaginationBaseRequestValidator());

        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Search text must be not empty if provided.")
            .MinimumLength(3).WithMessage("Search text must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Search text must not exceed 50 characters.")
            .Matches(RegexConstants.AlphanumericPlus).WithMessage("Search text contains invalid characters.")
            .When(x => x.SearchText != null);
    }
}
