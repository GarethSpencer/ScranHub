using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class SearchGroupsResponse : CommonPaginationResponse
{
    public IEnumerable<GroupResult>? Groups { get; set; }
}
