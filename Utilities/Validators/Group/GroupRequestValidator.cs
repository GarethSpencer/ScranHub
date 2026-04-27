using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities.Models.Requests;

namespace Utilities.Validators.Group;

public class GroupRequestValidator : AbstractValidator<GroupRequest>
{
    public GroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(30).WithMessage("Group name must not exceed 30 characters.");
    }
}
