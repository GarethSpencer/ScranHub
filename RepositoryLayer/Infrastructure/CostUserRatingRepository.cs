using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.CostUserRatings;

namespace RepositoryLayer.Infrastructure;

public sealed class CostUserRatingRepository(ScranHubDbContext dbContext) : EFRepository<CostUserRating>(dbContext), ICostUserRatingRepository
{
    public async Task<Guid> CreateAsync(Guid userId, CreateCostUserRatingRequest request, CancellationToken ct)
    {
        var costUserRating = new CostUserRating
        {
            UserId = userId,
            GroupVenueId = request.GroupVenueId,
            CostOptionId = request.CostOptionId
        };

        await _dbSet.AddAsync(costUserRating, ct);
        return costUserRating.CostUserRatingId;
    }
}