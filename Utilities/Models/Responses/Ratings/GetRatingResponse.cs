using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Ratings;

public class GetRatingResponse : CommonResponse
{
    public RatingResult? Rating { get; set; }
}
