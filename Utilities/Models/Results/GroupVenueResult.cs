namespace Utilities.Models.Results;

public record GroupVenueResult
{
    public required Guid GroupVenueId { get; init; }
    public required Guid GroupId { get; init; }
    public required string VenueName { get; init; }
    public required string VenueType { get; init; }
    public required string FoodType { get; init; }
    public required bool Visited { get; init; }
    public required decimal? AverageCostRating { get; init; }
    public required decimal? AverageQualityRating { get; init; }
    public required decimal? MyCostRating { get; init; }
    public required decimal? MyQualityRating { get; init; }
    public required int CostRatingVotes { get; init; }
    public required int QualityRatingVotes { get; init; }
    public required string? GooglePlaceId { get; init; }
    public required string? FormattedAddress { get; init; }
    public required decimal? Latitude { get; init; }
    public required decimal? Longitude { get; init; }
}
