namespace Utilities.Models.Results;

public record OptionResult
{
    public required Guid OptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
    public required int DisplayOrder { get; init; }
}
