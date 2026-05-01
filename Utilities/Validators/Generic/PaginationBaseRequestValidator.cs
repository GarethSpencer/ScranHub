using FluentValidation;
using Utilities.Models.Requests.Generic;

namespace Utilities.Validators.Generic;

public class PaginationBaseRequestValidator : AbstractValidator<PaginationBaseRequest>
{
    public PaginationBaseRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be a positive integer.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be a positive integer.");
    }
}
