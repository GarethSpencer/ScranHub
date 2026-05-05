using FluentValidation;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Generic;

namespace Utilities.Validators.Users;

public class SearchUserRequestValidator : AbstractValidator<SearchUserRequest>
{
    public SearchUserRequestValidator()
    {
        Include(new PaginationBaseRequestValidator());

        RuleFor(x => x.SearchText)
            .NotEmpty().WithMessage("Display name search text is required.")
            .Must(name => name == name.Trim()).WithMessage("Display name search text cannot have leading or trailing spaces.")
            .MinimumLength(3).WithMessage("Search text must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Search text must not exceed 30 characters.");
    }
}