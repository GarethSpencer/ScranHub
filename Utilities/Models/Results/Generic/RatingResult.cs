namespace Utilities.Models.Results.Generic;

public record RatingResult
{
    public required Guid RatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid GroupVenueId { get; init; }
    public required string VenueName { get; init; }
    public required Guid GroupId { get; init; }
    public required Guid OptionId { get; init; }
    public required string Label { get; init; }
}
