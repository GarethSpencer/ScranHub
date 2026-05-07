using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.QualityRatings;

public class AddQualityRatingResponse : CommonResponse
{
    public Guid? QualityRatingId { get; set; }
}
