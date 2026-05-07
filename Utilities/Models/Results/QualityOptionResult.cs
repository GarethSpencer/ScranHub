namespace Utilities.Models.Results;

public record QualityOptionResult
{
    public required Guid QualityOptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
    public required int DisplayOrder { get; init; }
}
