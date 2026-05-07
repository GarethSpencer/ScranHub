using Utilities.Models.Requests.CostUserRatings;
using Utilities.Models.Responses.CostUserRatings;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface ICostUserRatingService
{
    Task<AddCostUserRatingResponse> CreateCostUserRatingAsync(CreateCostUserRatingRequest request, CancellationToken ct);
}
