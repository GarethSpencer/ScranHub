using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class UserGroupsResponse : CommonResponse
{
    public Guid UserId { get; set; }
    public IEnumerable<UserGroupResult>? UserGroups { get; set; }
}
