namespace Utilities.Models.Results;

public record FriendResult
{
    public required Guid UserFriendId { get; init; }
    public required Guid FriendId { get; init; }
    public required bool Initiator { get; init; }
    public required bool Approved { get; init; }
    public required string DisplayName { get; init; }
    public required bool Active { get; init; }
}
