namespace Utilities.Models.Results;

public record FriendshipStatusResult
{
    public required int Value { get; init; }
    public required string Name { get; init; }
}
