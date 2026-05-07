using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.QualityRatings;

public class GetQualityRatingsResponse : CommonResponse
{
    public IEnumerable<QualityRatingResult>? QualityRatings { get; set; }
}
