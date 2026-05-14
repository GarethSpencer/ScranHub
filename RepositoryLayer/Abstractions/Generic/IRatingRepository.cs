using Utilities.Models.Requests.Ratings;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingRepository
{
    Task<Guid> CreateAsync(Guid userId, CreateRatingRequest request, CancellationToken ct);

    Task UpdateAsync(Guid ratingId, UpdateRatingRequest request, CancellationToken ct);

    Task<RatingResult?> GetDetailsByIdAsync(Guid ratingId, CancellationToken ct);

    Task DeleteAsync(Guid ratingId, CancellationToken ct);

    Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct);

    Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct);

    Task<bool> ExistsAsync(Guid groupVenueId, Guid userId, CancellationToken ct);

    Task<IEnumerable<RatingOptionResult>> GetDistinctRatingsGivenForGroupAsync(Guid groupId, CancellationToken ct);

    Task RemapRatingsMaintainDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct);

    Task RemapRatingsSquashDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct);
}
