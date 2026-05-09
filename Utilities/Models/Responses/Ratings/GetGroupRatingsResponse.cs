using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Ratings;

public class GetGroupRatingsResponse : CommonResponse
{
    public IEnumerable<GroupVenueRatingResult>? GroupVenueRatingsResults { get; set; }
}
