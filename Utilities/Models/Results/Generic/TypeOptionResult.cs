namespace Utilities.Models.Results.Generic;

public record TypeOptionResult
{
    public required Guid OptionId { get; init; }
    public required Guid? GroupId { get; init; }
    public required string Label { get; init; }
}
