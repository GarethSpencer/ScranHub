using FluentAssertions;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

public class SearchUserRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new SearchUserRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Big user name of 30 characters")]
    [InlineData("Bob")]
    [InlineData("User1_and_User2 Name?!")]
    [InlineData("User1 & User2's Name.")]
    [InlineData("Name ")]
    [InlineData(" Name")]
    public async Task ValidateAsync_ReturnsValidUserName(string userName)
    {
        var validator = new SearchUserRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = userName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("Bo", "3 characters")]
    [InlineData("Big users name of over 30 chars", "30 characters")]
    [InlineData("<Test User>", "invalid characters")]
    [InlineData("{Test User}", "invalid characters")]
    [InlineData("Test~User", "invalid characters")]
    [InlineData("Test|User", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidUserName(string userName, string error)
    {
        var validator = new SearchUserRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = userName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static SearchUserRequest CreateValidRequest() => new()
    {
        PageNumber = 1,
        PageSize = 10,
        SearchText = "Test User"
    };
}
