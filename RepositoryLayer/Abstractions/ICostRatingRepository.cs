using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.CostRatings;

namespace RepositoryLayer.Abstractions
{
    public interface ICostRatingRepository : IEFRepository<CostRating>
    {
        Task<Guid> CreateAsync(Guid userId, CreateCostRatingRequest request, CancellationToken ct);
    }
}