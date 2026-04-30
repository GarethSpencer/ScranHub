using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.Groups;

public class AddGroupResponse : CommonResponse
{
    public Guid? GroupId { get; set; }
}
