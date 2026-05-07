using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.QualityRatings;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class QualityRatingRepository(ScranHubDbContext dbContext) : EFRepository<QualityRating>(dbContext), IQualityRatingRepository
{
    public async Task<Guid> CreateAsync(Guid userId, CreateQualityRatingRequest request, CancellationToken ct)
    {
        var qualityRating = new QualityRating
        {
            UserId = userId,
            GroupVenueId = request.GroupVenueId,
            QualityOptionId = request.QualityOptionId
        };

        await _dbSet.AddAsync(qualityRating, ct);
        return qualityRating.QualityRatingId;
    }

    public async Task UpdateAsync(Guid qualityRatingId, UpdateQualityRatingRequest request, CancellationToken ct)
    {
        var qualityRating = await _dbSet.FindAsync([qualityRatingId], ct);
        qualityRating?.QualityOptionId = request.QualityOptionId;
    }

    public async Task<QualityRatingResult?> GetDetailsByIdAsync(Guid qualityRatingId, CancellationToken ct)
    {
        var qualityRating = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .FirstOrDefaultAsync(q => q.QualityRatingId == qualityRatingId, ct);

        if (qualityRating == null)
        {
            return null;
        }

        return new QualityRatingResult
        {
            QualityRatingId = qualityRating.QualityRatingId,
            UserId = qualityRating.UserId,
            GroupVenueId = qualityRating.GroupVenueId,
            VenueName = qualityRating.GroupVenue!.VenueName,
            GroupId = qualityRating.GroupVenue.GroupId,
            QualityOptionId = qualityRating.QualityOptionId,
            Label = qualityRating.QualityOption!.Label
        };
    }

    public async Task<IEnumerable<QualityRatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        var qualityRatings = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .Where(q => q.GroupVenueId == groupVenueId).ToListAsync(ct);

        if (qualityRatings == null || qualityRatings.Count == 0)
        {
            return [];
        }

        return qualityRatings.Select(qualityRating => new QualityRatingResult
        {
            QualityRatingId = qualityRating.QualityRatingId,
            UserId = qualityRating.UserId,
            GroupVenueId = qualityRating.GroupVenueId,
            VenueName = qualityRating.GroupVenue!.VenueName,
            GroupId = qualityRating.GroupVenue.GroupId,
            QualityOptionId = qualityRating.QualityOptionId,
            Label = qualityRating.QualityOption!.Label
        });
    }

    public async Task<IEnumerable<QualityRatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var qualityRatings = await _dbSet
            .Include(q => q.GroupVenue)
            .Include(q => q.QualityOption)
            .Where(q => q.UserId == userId && q.GroupVenue!.GroupId == groupId).ToListAsync(ct);

        if (qualityRatings == null || qualityRatings.Count == 0)
        {
            return [];
        }

        return qualityRatings.Select(qualityRating => new QualityRatingResult
        {
            QualityRatingId = qualityRating.QualityRatingId,
            UserId = qualityRating.UserId,
            GroupVenueId = qualityRating.GroupVenueId,
            VenueName = qualityRating.GroupVenue!.VenueName,
            GroupId = qualityRating.GroupVenue.GroupId,
            QualityOptionId = qualityRating.QualityOptionId,
            Label = qualityRating.QualityOption!.Label
        });
    }

    public async Task DeleteAsync(Guid qualityRatingId, CancellationToken ct)
    {
        var qualityRating = await _dbSet.FindAsync([qualityRatingId], ct);
        if (qualityRating != null)
        {
            _dbSet.Remove(qualityRating);
        }
    }
}