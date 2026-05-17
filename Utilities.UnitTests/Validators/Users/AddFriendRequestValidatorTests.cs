using FluentAssertions;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

[Trait("Category", "Unit")]
public class AddFriendRequestValidatorTests
{
    public readonly string Email256Chars = new string('a', 246) + "@email.com";
    public readonly string Email257Chars = new string('a', 247) + "@email.com";

    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new AddFriendRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("1@email.com")]
    [InlineData("helloitsme@hotmail.co.uk")]
    public async Task ValidateAsync_ReturnsValidEmail(string email)
    {
        var validator = new AddFriendRequestValidator();
        var request = CreateValidRequest();
        request.Email = email;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("test@email.com ", "leading or trailing spaces")]
    [InlineData(" test@email.com", "leading or trailing spaces")]
    [InlineData("testemail.com", "invalid")]
    [InlineData("@email", "invalid")]
    public async Task ValidateAsync_ReturnsInvalidEmail(string email, string error)
    {
        var validator = new AddFriendRequestValidator();
        var request = CreateValidRequest();
        request.Email = email;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsValidFor256CharEmail()
    {
        var validator = new AddFriendRequestValidator();
        var request = CreateValidRequest();
        request.Email = Email256Chars;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidFor257CharEmail()
    {
        var validator = new AddFriendRequestValidator();
        var request = CreateValidRequest();
        request.Email = Email257Chars;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("256 characters", StringComparison.InvariantCultureIgnoreCase));
    }

    private static AddFriendRequest CreateValidRequest() => new()
    {
        Email = "test@email.com"
    };
}
