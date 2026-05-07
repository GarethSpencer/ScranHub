namespace Utilities.Models.Results;

public record QualityRatingResult
{
    public required Guid QualityRatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required Guid GroupId { get; init; }
    public required Guid QualityOptionId { get; init; }
    public required string Label { get; init; }
}
