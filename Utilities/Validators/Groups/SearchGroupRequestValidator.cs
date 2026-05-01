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
            .MaximumLength(30).WithMessage("Search text must not exceed 30 characters.");
    }
}
