using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class CostRatingRepository(ScranHubDbContext dbContext)
    : RatingRepository<CostRating>(dbContext), ICostRatingRepository
{
    public override async Task<RatingResult?> GetDetailsByIdAsync(Guid costRatingId, CancellationToken ct)
    {
        var costRating = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .FirstOrDefaultAsync(c => c.CostRatingId == costRatingId, ct);

        if (costRating == null)
        {
            return null;
        }

        return MapToResult(costRating);
    }

    public override async Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenueId == groupVenueId).ToListAsync(ct);

        if (costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(MapToResult);
    }

    public override async Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.UserId == userId && c.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(MapToResult);
    }

    private static RatingResult MapToResult(CostRating c) => new()
    {
        RatingId = c.CostRatingId,
        UserId = c.UserId,
        GroupVenueId = c.GroupVenueId,
        VenueName = c.GroupVenue!.VenueName,
        GroupId = c.GroupVenue.GroupId,
        OptionId = c.CostOptionId,
        Label = c.CostOption!.Label
    };
}
