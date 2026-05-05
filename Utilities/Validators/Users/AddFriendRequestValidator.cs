using FluentValidation;
using Utilities.Models.Requests.Users;

namespace Utilities.Validators.Users;

public class AddFriendRequestValidator : AbstractValidator<AddFriendRequest>
{
    public AddFriendRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(email => email == email.Trim()).WithMessage("Email cannot have leading or trailing spaces.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Invalid email address.");
    }
}