using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.CostUserRatings;

public class AddCostUserRatingResponse : CommonResponse
{
    public Guid? CostUserRatingId { get; set; }
}
