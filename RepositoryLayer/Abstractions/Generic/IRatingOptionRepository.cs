using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingOptionRepository
{
    Task<IEnumerable<RatingOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);

    Task<IEnumerable<RatingOptionResult>> GetDefaultsAsync(CancellationToken ct);

    Task<bool> IsGroupUsingDefaultValues(Guid groupId, CancellationToken ct);

    Task<IEnumerable<Guid>> AddRangeAsync(SetOptionsRequest request, CancellationToken ct);

    Task RemoveCustomRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
