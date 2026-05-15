using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Options;

public class GetRatingOptionResponse : CommonResponse
{
    public RatingOptionResult? Option { get; set; }
}
