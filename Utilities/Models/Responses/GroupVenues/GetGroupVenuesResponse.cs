using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.GroupVenues;

public class GetGroupVenuesResponse : CommonPaginationResponse
{
    public IEnumerable<GroupVenueResult>? GroupVenues { get; set; }
}
