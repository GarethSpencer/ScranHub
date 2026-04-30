using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class SearchGroupsResponse : CommonResponse
{
    public IEnumerable<GroupResult>? Groups { get; set; }
}
