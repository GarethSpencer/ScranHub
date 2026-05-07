namespace Utilities.Models.Results;

public record GroupVenueQualityRatingResult
{
    public required Guid GroupId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required IEnumerable<QualityRatingVenueResult> QualityRatings { get; init; }
}
