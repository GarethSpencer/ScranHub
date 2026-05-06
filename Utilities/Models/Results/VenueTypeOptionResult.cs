namespace Utilities.Models.Results;

public record VenueTypeOptionResult
{
    public required Guid VenueTypeOptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
    public required int DisplayOrder { get; init; }
}
