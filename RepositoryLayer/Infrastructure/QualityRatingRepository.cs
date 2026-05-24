using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class QualityRatingRepository(ScranHubDbContext dbContext)
    : RatingRepository<QualityRating>(dbContext), IQualityRatingRepository
{
    public override async Task<RatingResult?> GetDetailsByIdAsync(Guid qualityRatingId, CancellationToken ct)
    {
        var qualityRating = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .FirstOrDefaultAsync(q => q.QualityRatingId == qualityRatingId, ct);

        if (qualityRating == null)
        {
            return null;
        }

        return MapToResult(qualityRating);
    }

    public override async Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        var qualityRatings = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .Where(q => q.GroupVenueId == groupVenueId)
            .OrderBy(q => q.User!.DisplayName)
            .ToListAsync(ct);

        if (qualityRatings.Count == 0)
        {
            return [];
        }

        return qualityRatings.Select(MapToResult);
    }

    public override async Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var qualityRatings = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .Where(q => q.UserId == userId && q.GroupVenue!.GroupId == groupId)
            .OrderBy(q => q.GroupVenue!.VenueName)
            .ToListAsync(ct);

        if (qualityRatings.Count == 0)
        {
            return [];
        }

        return qualityRatings.Select(MapToResult);
    }

    public override async Task<IEnumerable<RatingOptionResult>> GetDistinctRatingsGivenForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var ratings = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .Where(q => q.GroupVenue!.GroupId == groupId)
            .ToListAsync(ct);

        return [.. ratings.GroupBy(q => q.QualityOptionId)
            .Select(x => new RatingOptionResult
            {
                OptionId = x.Key,
                Label = x.First().QualityOption!.Label,
                GroupId = groupId,
                DisplayOrder = x.First().QualityOption!.DisplayOrder
            })];
    }

    public override async Task RemapRatingsMaintainDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct)
    {
        var ratingsToUpdate = _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.QualityOption)
            .Where(c => c.GroupVenue!.GroupId == groupId).ToList();

        foreach (var rating in ratingsToUpdate)
        {
            rating.QualityOptionId = optionIds.Skip(rating.QualityOption!.DisplayOrder - 1).First();
        }
    }

    public override async Task RemapRatingsSquashDisplayOrderAsync(Guid groupId, IEnumerable<Guid> optionIds, CancellationToken ct)
    {
        var ratingsToUpdate = _dbSet
            .Include(c => c.GroupVenue)
            .Include(c => c.QualityOption)
            .Where(c => c.GroupVenue!.GroupId == groupId).OrderBy(x => x.QualityOption!.DisplayOrder).ToList();

        var displayOrderToMap = 0;
        var lastDisplayOrder = 0;

        foreach (var rating in ratingsToUpdate)
        {
            if (rating.QualityOption!.DisplayOrder != lastDisplayOrder)
            {
                displayOrderToMap++;
            }
            lastDisplayOrder = rating.QualityOption.DisplayOrder;
            rating.QualityOptionId = optionIds.Skip(displayOrderToMap - 1).First();
        }
    }

    public override async Task<bool> IsOptionBeingUsedAsync(Guid optionId, CancellationToken ct)
    {
        var rating = await _dbSet.FirstOrDefaultAsync(r => r.QualityOptionId == optionId, ct);
        return rating != null;
    }

    private static RatingResult MapToResult(QualityRating q) => new()
    {
        RatingId = q.QualityRatingId,
        UserId = q.UserId,
        GroupVenueId = q.GroupVenueId,
        VenueName = q.GroupVenue!.VenueName,
        GroupId = q.GroupVenue.GroupId,
        OptionId = q.QualityOptionId,
        Label = q.QualityOption!.Label
    };
}