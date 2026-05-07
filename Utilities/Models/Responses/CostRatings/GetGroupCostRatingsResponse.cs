using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.CostRatings;

public class GetGroupCostRatingsResponse : CommonResponse
{
    public IEnumerable<GroupVenueCostRatingResult>? GroupVenueCostRatingsResults { get; set; }
}
