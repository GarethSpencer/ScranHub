using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.GroupVenues;

public class AddGroupVenueResponse : CommonResponse
{
    public Guid? GroupVenueId { get; set; }
}
