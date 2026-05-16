using FluentAssertions;
using Utilities.Models.Requests.Groups;
using Utilities.Validators.Groups;

namespace Utilities.UnitTests.Validators.Groups;

public class SearchGroupRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultGroupName()
    {
        var validator = new SearchGroupRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Big group name of 30 character")]
    [InlineData("G-1")]
    [InlineData("User1_and_User2 Group?!")]
    [InlineData("User1 & User2's Group.")]
    [InlineData("Test Group ")]
    [InlineData(" Test Group")]
    public async Task ValidateAsync_ReturnsValidGroupName(string groupName)
    {
        var validator = new SearchGroupRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = groupName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("TG", "3 characters")]
    [InlineData("Big group name of over 30 chars", "30 characters")]
    [InlineData("<Test Group>", "invalid characters")]
    [InlineData("{Test Group}", "invalid characters")]
    [InlineData("Test~Group", "invalid characters")]
    [InlineData("Test|Group", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidGroupName(string groupName, string error)
    {
        var validator = new SearchGroupRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = groupName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static SearchGroupRequest CreateValidRequest() => new()
    {
        PageNumber = 1,
        PageSize = 10,
        SearchText = "Test Group"
    };
}
