using FluentAssertions;
using Utilities.Enums;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators.GroupVenues;

namespace Utilities.UnitTests.Validators.GroupVenues;

[Trait("Category", "Unit")]
public class SortableGroupVenueRequestValidatorTests
{
    [Theory]
    [InlineData(GroupVenueSortParameters.VenueName)]
    [InlineData(GroupVenueSortParameters.FoodType)]
    [InlineData(GroupVenueSortParameters.Visited)]
    [InlineData(GroupVenueSortParameters.VenueType)]
    public async Task ValidateAsync_ReturnsValidVenueName(GroupVenueSortParameters sortBy)
    {
        var validator = new SortableGroupVenueRequestValidator();
        var request = CreateValidRequest();
        request.SortBy = sortBy;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static SortableGroupVenueRequest CreateValidRequest() => new()
    {
        PageSize = 10,
        PageNumber = 1,
        SortDescending = false,
        SortBy = GroupVenueSortParameters.VenueName
    };
}
