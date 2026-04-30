using FluentValidation;
using Utilities.Models.Requests.Groups;

namespace Utilities.Validators.Group;

public class SearchGroupRequestValidator : AbstractValidator<SearchGroupRequest>
{
    public SearchGroupRequestValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Search text is required.")
            .MaximumLength(30).WithMessage("Search text must not exceed 30 characters.");
    }
}
