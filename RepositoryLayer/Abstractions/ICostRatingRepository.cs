using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.CostRatings;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface ICostRatingRepository : IEFRepository<CostRating>
    {
        Task<Guid> CreateAsync(Guid userId, CreateCostRatingRequest request, CancellationToken ct);

        Task UpdateAsync(Guid costRatingId, UpdateCostRatingRequest request, CancellationToken ct);

        Task<CostRatingResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct);

        Task DeleteAsync(Guid costRatingId, CancellationToken ct);
    }
}