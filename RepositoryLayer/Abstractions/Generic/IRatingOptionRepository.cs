using DAL.Entities.Abstractions;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingOptionRepository
{
    Task<IEnumerable<RatingOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
}
