using Utilities.Models.Responses.Generic;
using Utilities.Models.Results;

namespace Utilities.Models.Responses.QualityRatings;

public class GetGroupQualityRatingsResponse : CommonResponse
{
    public IEnumerable<GroupVenueQualityRatingResult>? GroupVenueQualityRatingsResults { get; set; }
}
