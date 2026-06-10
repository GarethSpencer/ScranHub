using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Users;

public class GetUserDetailedResponse : CommonResponse
{
    public UserDetailedResult? User { get; set; }
}
