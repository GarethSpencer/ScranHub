using Utilities.Enums;

namespace Utilities.Models.Results;

public record UserFriendResult
{
    public required Guid UserFriendId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid FriendId { get; init; }
    public required FriendshipStatus Status { get; init; }
}
