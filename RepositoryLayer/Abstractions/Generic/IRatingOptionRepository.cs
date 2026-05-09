using DAL.Entities.Abstractions;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingOptionRepository<TRatingOption> : IEFRepository<TRatingOption>
    where TRatingOption : class, IRatingOption
{
    Task<IEnumerable<RatingOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
}
