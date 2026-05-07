using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.CostRatings;

public class GetCostRatingResponse : CommonResponse
{
    public CostRatingResult? CostRating { get; set; }
}
