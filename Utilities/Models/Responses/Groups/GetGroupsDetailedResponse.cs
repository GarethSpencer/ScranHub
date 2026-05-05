using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class GetGroupsDetailedResponse : CommonPaginationResponse
{
    public IEnumerable<GroupDetailedResult>? Groups { get; set; }
}
