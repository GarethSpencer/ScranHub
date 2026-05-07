using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
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

    public async Task<CostRatingResult?> GetDetailsByIdAsync(Guid costRatingId, CancellationToken ct)
    {
        var costRating = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .FirstOrDefaultAsync(c => c.CostRatingId == costRatingId, ct);

        if (costRating == null)
        {
            return null;
        }

        return new CostRatingResult
        {
            CostRatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            CostOptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        };
    }

    public async Task<IEnumerable<CostRatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenueId == groupVenueId).ToListAsync(ct);

        if (costRatings == null || costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(costRating => new CostRatingResult
        {
            CostRatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            CostOptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        });
    }

    public async Task<IEnumerable<CostRatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.UserId == userId && c.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (costRatings == null || costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(costRating => new CostRatingResult
        {
            CostRatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            CostOptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        });
    }

    public async Task<IEnumerable<GroupVenueCostRatingResult>> GetDetailsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (costRatings == null || costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(costRating => new GroupVenueCostRatingResult
        {
            GroupId = costRating.GroupVenue!.GroupId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            CostRatings = costRatings.Where(cr => cr.GroupVenueId == costRating.GroupVenueId).Select(cr => new CostRatingVenueResult
            {
                CostRatingId = cr.CostRatingId,
                UserId = cr.UserId,
                CostOptionId = cr.CostOptionId,
                Label = cr.CostOption!.Label
            })
        });
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