namespace Utilities.Models.Results;

public record GroupVenueCostRatingResult
{
    public required Guid GroupId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required IEnumerable<CostRatingVenueResult> CostRatings { get; init; }
}
