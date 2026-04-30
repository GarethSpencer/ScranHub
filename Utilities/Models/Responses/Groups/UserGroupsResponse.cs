using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class UserGroupsResponse : CommonResponse
{
    public Guid? UserId { get; set; }
    public IEnumerable<GroupResult>? UserGroups { get; set; }
}
