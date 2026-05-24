using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingService
{
    Task<CommonResponse> CreateRatingAsync(CreateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteRatingAsync(Guid ratingId, CancellationToken ct);
    Task<CommonResponse> GetRatingAsync(Guid ratingId, CancellationToken ct);
    Task<CommonResponse> GetRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<CommonResponse> GetUserRatingsForGroupAsync(Guid groupId, CancellationToken ct);
    Task<CommonResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
