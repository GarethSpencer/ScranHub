using FluentAssertions;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators.GroupVenues;

namespace Utilities.UnitTests.Validators.GroupVenues;

[Trait("Category", "Unit")]
public class SearchGroupVenueRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new SearchGroupVenueRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("This is a very very big venue name of 50 character")]
    [InlineData("G-1")]
    [InlineData("User1_and_User2 Venue?!")]
    [InlineData("User1 & User2's Venue.")]
    [InlineData("Test Venue ")]
    [InlineData(" Test Venue")]
    public async Task ValidateAsync_ReturnsValidVenueName(string venueName)
    {
        var validator = new SearchGroupVenueRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = venueName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "not empty")]
    [InlineData("TG", "3 characters")]
    [InlineData("This is a very very big venue name of over 50 chars", "50 characters")]
    [InlineData("<Test Venue>", "invalid characters")]
    [InlineData("{Test Venue}", "invalid characters")]
    [InlineData("Test~Venue", "invalid characters")]
    [InlineData("Test|Venue", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidVenueName(string venueName, string error)
    {
        var validator = new SearchGroupVenueRequestValidator();
        var request = CreateValidRequest();
        request.SearchText = venueName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static SearchGroupVenueRequest CreateValidRequest() => new()
    {
        SearchText = "Test Venue",
        PageNumber = 1,
        PageSize = 10
    };
}
