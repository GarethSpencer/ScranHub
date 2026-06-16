using FluentValidation;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Generic;

namespace Utilities.Validators.Users;

public class GetUserFriendRequestValidator : AbstractValidator<GetUserFriendRequest>
{
    public GetUserFriendRequestValidator()
    {
        Include(new PaginationBaseRequestValidator());

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid friendship status.");
    }
}