using Utilities.Enums;
using Utilities.Models.Requests.Generic;

namespace Utilities.Models.Requests.Users;

public record GetUserFriendRequest : PaginationBaseRequest
{
    public required FriendshipStatus Status { get; set; }
}
