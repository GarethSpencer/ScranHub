namespace Utilities.Models.Results;

public record CostRatingResult
{
    public required Guid CostRatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required Guid GroupId { get; init; }
    public required Guid CostOptionId { get; init; }
    public required string Label { get; init; }
}
