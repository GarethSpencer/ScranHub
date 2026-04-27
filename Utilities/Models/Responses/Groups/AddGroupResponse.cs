using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class AddGroupResponse : CommonResponse
{
    public Guid? GroupId { get; set; }
}
