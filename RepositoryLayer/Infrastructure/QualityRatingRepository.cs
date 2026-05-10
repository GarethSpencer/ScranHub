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
            .Where(q => q.GroupVenueId == groupVenueId).ToListAsync(ct);

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
            .Where(q => q.UserId == userId && q.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (qualityRatings.Count == 0)
        {
            return [];
        }

        return qualityRatings.Select(MapToResult);
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