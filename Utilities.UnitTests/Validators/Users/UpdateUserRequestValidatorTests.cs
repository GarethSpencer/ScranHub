using FluentAssertions;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

public class UpdateUserRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new UpdateUserRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Large Display Name of 30 chars")]
    [InlineData("D-1")]
    [InlineData("User1_Name?!")]
    [InlineData("User1's Name.")]
    public async Task ValidateAsync_ReturnsValidDisplayName(string name)
    {
        var validator = new UpdateUserRequestValidator();
        var request = CreateValidRequest();
        request.DisplayName = name;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("Test Name ", "leading or trailing spaces")]
    [InlineData(" Test Name", "leading or trailing spaces")]
    [InlineData("TN", "3 characters")]
    [InlineData("Big DisplayName of over 30 char", "30 characters")]
    [InlineData("<Test Name>", "invalid characters")]
    [InlineData("{Test Name}", "invalid characters")]
    [InlineData("Test~Name", "invalid characters")]
    [InlineData("Test|Name", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidDisplayName(string name, string error)
    {
        var validator = new UpdateUserRequestValidator();
        var request = CreateValidRequest();
        request.DisplayName = name;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static UpdateUserRequest CreateValidRequest() => new()
    {
        DisplayName = "Test User",
        Admin = true,
        Active = true
    };
}
