using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class GetGroupResponse : CommonResponse
{
    public GroupResult? Group { get; set; }
}
