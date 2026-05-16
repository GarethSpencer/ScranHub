using FluentAssertions;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators.GroupVenues;

namespace Utilities.UnitTests.Validators.GroupVenues;

public class UpdateGroupVenueRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithDefaultRequest()
    {
        var validator = new UpdateGroupVenueRequestValidator();
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
    public async Task ValidateAsync_ReturnsValidVenueName(string venueName)
    {
        var validator = new UpdateGroupVenueRequestValidator();
        var request = CreateValidRequest();
        request.VenueName = venueName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "required")]
    [InlineData("Test Venue ", "leading or trailing spaces")]
    [InlineData(" Test Venue", "leading or trailing spaces")]
    [InlineData("TG", "3 characters")]
    [InlineData("This is a very very big venue name of over 50 chars", "50 characters")]
    [InlineData("<Test Venue>", "invalid characters")]
    [InlineData("{Test Venue}", "invalid characters")]
    [InlineData("Test~Venue", "invalid characters")]
    [InlineData("Test|Venue", "invalid characters")]
    public async Task ValidateAsync_ReturnsInvalidVenueName(string venueName, string error)
    {
        var validator = new UpdateGroupVenueRequestValidator();
        var request = CreateValidRequest();
        request.VenueName = venueName;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(error, StringComparison.InvariantCultureIgnoreCase));
    }

    private static UpdateGroupVenueRequest CreateValidRequest() => new()
    {
        VenueName = "Test Venue",
        Visited = true,
        FoodTypeOptionId = Guid.Empty,
        VenueTypeOptionId = Guid.Empty
    };
}
