using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Users;

public class UserFriendsResponse : CommonPaginationResponse
{
    public Guid? UserId { get; set; }
    public IEnumerable<FriendResult>? Friends { get; set; }
}
