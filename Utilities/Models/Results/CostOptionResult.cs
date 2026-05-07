namespace Utilities.Models.Results;

public record CostOptionResult
{
    public required Guid CostOptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
    public required int DisplayOrder { get; init; }
}
