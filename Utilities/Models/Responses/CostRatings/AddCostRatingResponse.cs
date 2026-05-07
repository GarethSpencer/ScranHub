using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.CostRatings;

public class AddCostRatingResponse : CommonResponse
{
    public Guid? CostRatingId { get; set; }
}
