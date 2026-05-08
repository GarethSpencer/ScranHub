using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.CostRatings;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface ICostRatingRepository : IEFRepository<CostRating>
{
    Task<Guid> CreateAsync(Guid userId, CreateCostRatingRequest request, CancellationToken ct);

    Task UpdateAsync(Guid costRatingId, UpdateCostRatingRequest request, CancellationToken ct);

    Task<CostRatingResult?> GetDetailsByIdAsync(Guid costRatingId, CancellationToken ct);

    Task DeleteAsync(Guid costRatingId, CancellationToken ct);

    Task<IEnumerable<CostRatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct);

    Task<IEnumerable<CostRatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct);
}
