using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.QualityRatings;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IQualityRatingRepository : IEFRepository<QualityRating>
    {
        Task<Guid> CreateAsync(Guid userId, CreateQualityRatingRequest request, CancellationToken ct);

        Task UpdateAsync(Guid qualityRatingId, UpdateQualityRatingRequest request, CancellationToken ct);

        Task<QualityRatingResult?> GetDetailsByIdAsync(Guid qualityRatingId, CancellationToken ct);

        Task DeleteAsync(Guid qualityRatingId, CancellationToken ct);

        Task<IEnumerable<QualityRatingResult>> GetDetailsByGroupVenueIdAsync(Guid groupVenueId, CancellationToken ct);

        Task<IEnumerable<QualityRatingResult>> GetUserDetailsForGroupAsync(Guid userId, Guid groupId, CancellationToken ct);
    }
}