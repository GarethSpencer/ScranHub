using FluentValidation;
using Utilities.Models.Requests.Users;

namespace Utilities.Validators.Users;

public class UpdateUserFriendRequestValidator : AbstractValidator<UpdateUserFriendRequest>
{
    public UpdateUserFriendRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid friendship status.");
    }
}
