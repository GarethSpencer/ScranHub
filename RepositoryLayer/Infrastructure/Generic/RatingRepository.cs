using DAL.Data;
using DAL.Entities.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public abstract class RatingRepository<TRating>(ScranHubDbContext dbContext) : EFRepository<TRating>(dbContext), IRatingRepository<TRating>
        where TRating : class, IRating, new()
{
    public async Task<Guid> CreateAsync(Guid userId, CreateRatingRequest request, CancellationToken ct)
    {
        var rating = new TRating
        {
            UserId = userId,
            GroupVenueId = request.GroupVenueId,
            OptionId = request.OptionId
        };

        await _dbSet.AddAsync(rating, ct);
        return rating.RatingId;
    }

    public async Task UpdateAsync(Guid ratingId, UpdateRatingRequest request, CancellationToken ct)
    {
        var rating = await _dbSet.FindAsync([ratingId], ct);
        rating?.OptionId = request.OptionId;
    }

    public async Task DeleteAsync(Guid ratingId, CancellationToken ct)
    {
        var rating = await _dbSet.FindAsync([ratingId], ct);
        if (rating != null)
        {
            _dbSet.Remove(rating);
        }
    }

    public abstract Task<RatingResult?> GetDetailsByIdAsync(Guid ratingId, CancellationToken ct);

    public abstract Task<IEnumerable<RatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct);

    public abstract Task<IEnumerable<RatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct);
}