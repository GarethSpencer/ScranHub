namespace Utilities.Models.Results;

public record GroupVenueResult
{
    public required Guid GroupVenueId { get; init; }
    public required Guid GroupId { get; init; }
    public required string VenueName { get; init; }
    public required string VenueType { get; init; }
    public required string FoodType { get; init; }
    public required bool Visited { get; init; }
}
