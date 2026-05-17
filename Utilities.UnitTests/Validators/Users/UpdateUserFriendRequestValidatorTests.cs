using FluentAssertions;
using Utilities.Enums;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

[Trait("Category", "Unit")]
public class UpdateUserFriendRequestValidatorTests
{
    [Theory]
    [InlineData(FriendshipStatus.Declined)]
    [InlineData(FriendshipStatus.Pending)]
    [InlineData(FriendshipStatus.Accepted)]
    public async Task ValidateAsync_ReturnsValidWithAnyFriendshipStatus(FriendshipStatus status)
    {
        var validator = new UpdateUserFriendRequestValidator();
        var request = new UpdateUserFriendRequest
        {
            Status = status
        };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
