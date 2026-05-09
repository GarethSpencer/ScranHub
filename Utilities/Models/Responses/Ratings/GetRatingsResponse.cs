using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Ratings;

public class GetRatingsResponse : CommonResponse
{
    public IEnumerable<RatingResult>? Ratings { get; set; }
}
