using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class CostRatingRepository(ScranHubDbContext dbContext) : RatingRepository<CostRating>(dbContext), ICostRatingRepository
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

        return new RatingResult
        {
            RatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            OptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        };
    }

    public override async Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenueId == groupVenueId).ToListAsync(ct);

        if (costRatings == null || costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(costRating => new RatingResult
        {
            RatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            OptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        });
    }

    public override async Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var costRatings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.UserId == userId && c.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (costRatings == null || costRatings.Count == 0)
        {
            return [];
        }

        return costRatings.Select(costRating => new RatingResult
        {
            RatingId = costRating.CostRatingId,
            UserId = costRating.UserId,
            GroupVenueId = costRating.GroupVenueId,
            VenueName = costRating.GroupVenue!.VenueName,
            GroupId = costRating.GroupVenue.GroupId,
            OptionId = costRating.CostOptionId,
            Label = costRating.CostOption!.Label
        });
    }
}
