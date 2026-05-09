namespace Utilities.Models.Results.Generic;

public record GroupVenueRatingResult
{
    public required Guid GroupId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required IEnumerable<RatingVenueResult> Ratings { get; init; }
}
