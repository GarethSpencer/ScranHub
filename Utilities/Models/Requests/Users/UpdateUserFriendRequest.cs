using Utilities.Enums;

namespace Utilities.Models.Requests.Users;

public record UpdateUserFriendRequest
{
    public required FriendshipStatus Status { get; set; }
}
