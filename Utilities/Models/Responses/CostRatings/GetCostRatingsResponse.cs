using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.CostRatings;

public class GetCostRatingsResponse : CommonResponse
{
    public IEnumerable<CostRatingResult>? CostRatings { get; set; }
}
