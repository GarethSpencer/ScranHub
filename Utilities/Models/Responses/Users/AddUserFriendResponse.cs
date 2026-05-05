using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Users;

public class AddUserFriendResponse : CommonResponse
{
    public Guid? UserFriendId { get; set; }
}
