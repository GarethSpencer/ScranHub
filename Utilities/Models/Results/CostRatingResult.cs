namespace Utilities.Models.Results;

public record CostRatingResult
{
    public required Guid CostRatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required Guid CostOptionId { get; init; }
}
