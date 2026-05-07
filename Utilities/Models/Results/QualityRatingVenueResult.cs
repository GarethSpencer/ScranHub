namespace Utilities.Models.Results;

public record QualityRatingVenueResult
{
    public required Guid QualityRatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid QualityOptionId { get; init; }
    public required string Label { get; init; }
}
