using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Options;

public class GetRatingOptionsResponse : CommonResponse
{
    public IEnumerable<RatingOptionResult>? Options { get; set; }
}
