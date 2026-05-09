using DAL.Entities.Abstractions;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingRepository<TRating> : IEFRepository<TRating>
    where TRating : class, IRating
{
    Task<Guid> CreateAsync(Guid userId, CreateRatingRequest request, CancellationToken ct);

    Task UpdateAsync(Guid qualityRatingId, UpdateRatingRequest request, CancellationToken ct);

    Task<RatingResult?> GetDetailsByIdAsync(Guid qualityRatingId, CancellationToken ct);

    Task DeleteAsync(Guid qualityRatingId, CancellationToken ct);

    Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct);

    Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct);
}
