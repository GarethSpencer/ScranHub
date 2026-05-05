using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class GetGroupsResponse : CommonPaginationResponse
{
    public IEnumerable<GroupResult>? Groups { get; set; }
}
