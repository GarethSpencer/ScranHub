using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using System.Text.RegularExpressions;
using Utilities.Models.Requests.CostRatings;
using Utilities.Models.Results;

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

    public async Task UpdateAsync(Guid costRatingId, UpdateCostRatingRequest request, CancellationToken ct)
    {
        var costRating = await _dbSet.FindAsync([costRatingId], ct);
        costRating?.CostOptionId = request.CostOptionId;
    }

    public async Task<CostRatingResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct)
    {
        var costRating = await _dbSet.FindAsync([id], ct);

        if (costRating == null)
        {
            return null;
        }

        return new CostRatingResult
        {
            CostRatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            CostOptionId = costRating.CostOptionId,
        };
    }

    public async Task DeleteAsync(Guid costRatingId, CancellationToken ct)
    {
        var costRating = await _dbSet.FindAsync([costRatingId], ct);
        if (costRating != null)
        {
            _dbSet.Remove(costRating);
        }
    }
}