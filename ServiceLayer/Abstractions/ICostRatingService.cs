using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;

namespace ServiceLayer.Abstractions;

public interface ICostRatingService
{
    Task<AddRatingResponse> CreateCostRatingAsync(CreateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateCostRatingAsync(Guid costRatingId, UpdateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteCostRatingAsync(Guid costRatingId, CancellationToken ct);
    Task<GetRatingResponse> GetCostRatingAsync(Guid costRatingId, CancellationToken ct);
    Task<GetRatingsResponse> GetCostRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<GetRatingsResponse> GetUserCostRatingsForGroupAsync(Guid groupId, CancellationToken ct);
    Task<GetGroupRatingsResponse> GetCostRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
