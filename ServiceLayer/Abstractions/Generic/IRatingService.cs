using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingService
{
    Task<AddRatingResponse> CreateRatingAsync(CreateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateRatingAsync(Guid qualityRatingId, UpdateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetRatingResponse> GetRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetRatingsResponse> GetRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<GetRatingsResponse> GetUserRatingsForGroupAsync(Guid groupId, CancellationToken ct);
    Task<GetGroupRatingsResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
