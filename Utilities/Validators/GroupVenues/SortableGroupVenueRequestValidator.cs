using FluentValidation;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators.Generic;

namespace Utilities.Validators.GroupVenues;

public class SortableGroupVenueRequestValidator : AbstractValidator<SortableGroupVenueRequest>
{
    public SortableGroupVenueRequestValidator()
    {
        Include(new PaginationBaseRequestValidator());

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("Invalid sort field value.");
    }
}
