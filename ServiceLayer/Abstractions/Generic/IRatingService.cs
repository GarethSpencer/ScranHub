using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingService
{
    Task<CommonResponse> CreateRatingAsync(CreateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateRatingAsync(Guid qualityRatingId, UpdateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<CommonResponse> GetRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<CommonResponse> GetRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<CommonResponse> GetUserRatingsForGroupAsync(Guid groupId, CancellationToken ct);
    Task<CommonResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
