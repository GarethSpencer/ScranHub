using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Users;

public class GetUsersResponse : CommonPaginationResponse
{
    public IEnumerable<UserResult>? Users { get; set; }
}
