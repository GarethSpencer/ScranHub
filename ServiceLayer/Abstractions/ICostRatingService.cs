using Utilities.Models.Requests.CostRatings;
using Utilities.Models.Responses.CostRatings;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface ICostRatingService
{
    Task<AddCostRatingResponse> CreateCostRatingAsync(CreateCostRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateCostRatingAsync(Guid costRatingId, UpdateCostRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteCostRatingAsync(Guid costRatingId, CancellationToken ct);
}
