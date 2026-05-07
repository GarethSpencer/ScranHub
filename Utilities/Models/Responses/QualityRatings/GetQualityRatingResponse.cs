using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.QualityRatings;

public class GetQualityRatingResponse : CommonResponse
{
    public QualityRatingResult? QualityRating { get; set; }
}
