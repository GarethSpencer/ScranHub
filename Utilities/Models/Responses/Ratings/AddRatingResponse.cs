using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Ratings;

public class AddRatingResponse : CommonResponse
{
    public Guid? RatingId { get; set; }
}
