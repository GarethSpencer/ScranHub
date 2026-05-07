using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.CostUserRatings;

namespace RepositoryLayer.Abstractions
{
    public interface ICostUserRatingRepository : IEFRepository<CostUserRating>
    {
        Task<Guid> CreateAsync(Guid userId, CreateCostUserRatingRequest request, CancellationToken ct);
    }
}