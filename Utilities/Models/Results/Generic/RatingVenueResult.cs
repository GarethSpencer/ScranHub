namespace Utilities.Models.Results.Generic;

public record RatingVenueResult
{
    public required Guid RatingId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid OptionId { get; init; }
    public required string Label { get; init; }
    public required string DisplayName { get; init; }
}
