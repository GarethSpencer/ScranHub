using FluentAssertions;
using Utilities.Models.Requests.Options;
using Utilities.Validators.Options;

namespace Utilities.UnitTests.Validators.Options;

public class UpdateOptionRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new UpdateOptionRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Big label name of 30 character")]
    [InlineData("L-1")]
    [InlineData("User1_and_User2 Label?!")]
    [InlineData("User1 & User2's Label.")]
    [InlineData("TL")]
    public async Task ValidateAsync_ReturnsValidOptionLabel(string label)
    {
        var validator = new UpdateOptionRequestValidator();
        var request = CreateValidRequest();
        request.Label = label;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("Test Label ", "leading or trailing spaces")]
    [InlineData(" Test Label", "leading or trailing spaces")]
    [InlineData("Big label name of over 30 chars", "30 characters")]
    [InlineData("<Test Label>", "invalid characters")]
    [InlineData("{Test Label}", "invalid characters")]
    [InlineData("Test~Label", "invalid characters")]
    [InlineData("Test|Label", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidOptionLabel(string label, string error)
    {
        var validator = new UpdateOptionRequestValidator();
        var request = CreateValidRequest();
        request.Label = label;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static UpdateOptionRequest CreateValidRequest() => new()
    {
        Label = "Test Label"
    };
}
