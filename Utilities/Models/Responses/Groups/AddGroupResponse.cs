using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Groups;

public class AddGroupResponse : CommonResponse
{
    public Guid? GroupId { get; set; }
}
