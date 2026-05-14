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

    public override async Task<IEnumerable<RatingOptionResult>> GetDistinctRatingsGivenForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var ratings = await _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenue!.GroupId == groupId)
            .ToListAsync(ct);

        return [.. ratings.GroupBy(c => c.CostOptionId)
            .Select(x => new RatingOptionResult
            {
                OptionId = x.Key,
                Label = x.First().CostOption!.Label,
                GroupId = groupId,
                DisplayOrder = x.First().CostOption!.DisplayOrder
            })];
    }

    public override async Task RemapRatingsMaintainDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct)
    {
        var ratingsToUpdate = _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenue!.GroupId == groupId).ToList();

        foreach (var rating in ratingsToUpdate)
        {
            rating.CostRatingId = optionIds.Skip(rating.CostOption!.DisplayOrder - 1).First();
        }
    }

    public override async Task RemapRatingsSquashDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct)
    {
        var ratingsToUpdate = _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.CostOption)
            .Where(c => c.GroupVenue!.GroupId == groupId).OrderBy(x => x.CostOption!.DisplayOrder).ToList();

        var displayOrderToMap = 0;
        var lastDisplayOrder = 0;

        foreach (var rating in ratingsToUpdate)
        {
            if (rating.CostOption!.DisplayOrder != lastDisplayOrder)
            {
                displayOrderToMap++;
            }
            lastDisplayOrder = rating.CostOption.DisplayOrder;
            rating.CostRatingId = optionIds.Skip(displayOrderToMap - 1).First();
        }
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
