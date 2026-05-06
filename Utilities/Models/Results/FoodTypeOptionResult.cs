namespace Utilities.Models.Results;

public record FoodTypeOptionResult
{
    public required Guid FoodTypeOptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
    public required int DisplayOrder { get; init; }
}
