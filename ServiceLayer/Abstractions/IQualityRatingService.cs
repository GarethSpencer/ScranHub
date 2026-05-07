using Utilities.Models.Requests.QualityRatings;
using Utilities.Models.Responses.QualityRatings;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface IQualityRatingService
{
    Task<AddQualityRatingResponse> CreateQualityRatingAsync(CreateQualityRatingRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateQualityRatingAsync(Guid qualityRatingId, UpdateQualityRatingRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteQualityRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetQualityRatingResponse> GetQualityRatingAsync(Guid qualityRatingId, CancellationToken ct);
    Task<GetQualityRatingsResponse> GetQualityRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
    Task<GetQualityRatingsResponse> GetUserQualityRatingsForGroupAsync  (Guid groupId, CancellationToken ct);
    Task<GetGroupQualityRatingsResponse> GetQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
