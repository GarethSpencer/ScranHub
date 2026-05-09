using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;

namespace ServiceLayer.Abstractions;

public interface IQualityRatingService
{
    Task<AddRatingResponse> CreateQualityRatingAsync(CreateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateQualityRatingAsync(Guid qualityRatingId, UpdateRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteQualityRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetRatingResponse> GetQualityRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetRatingsResponse> GetQualityRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<GetRatingsResponse> GetUserQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct);
    Task<GetGroupRatingsResponse> GetQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
