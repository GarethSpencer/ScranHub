using FluentAssertions;
using Utilities.Enums;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

[Trait("Category", "Unit")]
public class GetUserFriendRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new GetUserFriendRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(FriendshipStatus.Pending)]
    [InlineData(FriendshipStatus.Accepted)]
    [InlineData(FriendshipStatus.Declined)]
    public async Task ValidateAsync_ReturnsValidFriendshipStatus(FriendshipStatus status)
    {
        var validator = new GetUserFriendRequestValidator();
        var request = CreateValidRequest();
        request.FriendshipStatus = status;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-1)]
    public async Task ValidateAsync_ReturnsInvalidUserName(int status)
    {
        var validator = new GetUserFriendRequestValidator();
        var request = CreateValidRequest();
        request.FriendshipStatus = (FriendshipStatus)status;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("FriendshipStatus must be a valid status", StringComparison.InvariantCultureIgnoreCase));
    }

    private static GetUserFriendRequest CreateValidRequest() => new()
    {
        PageNumber = 1,
        PageSize = 10,
        FriendshipStatus = FriendshipStatus.Pending
    };
}
