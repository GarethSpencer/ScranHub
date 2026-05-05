using FluentValidation;
using Utilities.Models.Requests.Groups;
using Utilities.Validators.Generic;

namespace Utilities.Validators.Groups;

public class SearchGroupRequestValidator : AbstractValidator<SearchGroupRequest>
{
    public SearchGroupRequestValidator()
    {
        Include(new PaginationBaseRequestValidator());

        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Search text is required.")
            .MinimumLength(3).WithMessage("Search text must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Search text must not exceed 30 characters.");
    }
}
