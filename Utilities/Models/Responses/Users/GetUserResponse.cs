using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Users;

public class GetUserResponse : CommonResponse
{
    public UserResult? User { get; set; }
}
