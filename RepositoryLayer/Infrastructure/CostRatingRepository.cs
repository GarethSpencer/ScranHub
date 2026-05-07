using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.CostRatings;

namespace RepositoryLayer.Infrastructure;

public sealed class CostRatingRepository(ScranHubDbContext dbContext) : EFRepository<CostRating>(dbContext), ICostRatingRepository
{
    public async Task<Guid> CreateAsync(Guid userId, CreateCostRatingRequest request, CancellationToken ct)
    {
        var costRating = new CostRating
        {
            UserId = userId,
            GroupVenueId = request.GroupVenueId,
            CostOptionId = request.CostOptionId
        };

        await _dbSet.AddAsync(costRating, ct);
        return costRating.CostRatingId;
    }
}