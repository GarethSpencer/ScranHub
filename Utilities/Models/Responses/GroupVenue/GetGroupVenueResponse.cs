using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.GroupVenue;

public class GetGroupVenueResponse : CommonResponse
{
    public GroupVenueResult? GroupVenue { get; set; }
}
