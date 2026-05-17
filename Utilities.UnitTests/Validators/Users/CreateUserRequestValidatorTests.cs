using FluentAssertions;
using Utilities.Models.Requests.Users;
using Utilities.Validators.Users;

namespace Utilities.UnitTests.Validators.Users;

[Trait("Category", "Unit")]
public class CreateUserRequestValidatorTests
{
    public readonly string Email256Chars = new string('a', 246) + "@email.com";
    public readonly string Email257Chars = new string('a', 247) + "@email.com";

    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new CreateUserRequestValidator();
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
        var validator = new CreateUserRequestValidator();
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
        var validator = new CreateUserRequestValidator();
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
        var validator = new CreateUserRequestValidator();
        var request = CreateValidRequest();
        request.Email = Email256Chars;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidFor257CharEmail()
    {
        var validator = new CreateUserRequestValidator();
        var request = CreateValidRequest();
        request.Email = Email257Chars;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("256 characters", StringComparison.InvariantCultureIgnoreCase));
    }

    [Theory]
    [InlineData("Large Display Name of 30 chars")]
    [InlineData("D-1")]
    [InlineData("User1_Name?!")]
    [InlineData("User1's Name.")]
    public async Task ValidateAsync_ReturnsValidDisplayName(string name)
    {
        var validator = new CreateUserRequestValidator();
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
        var validator = new CreateUserRequestValidator();
        var request = CreateValidRequest();
        request.DisplayName = name;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static CreateUserRequest CreateValidRequest() => new()
    {
        Email = "test@email.com",
        DisplayName = "Test User",
        Admin = true
    };
}
