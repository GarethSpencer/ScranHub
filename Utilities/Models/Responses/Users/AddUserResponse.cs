using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Users;

public class AddUserResponse : CommonResponse
{
    public Guid? UserId { get; set; }
}
