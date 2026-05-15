using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface IRatingOptionRepository
{
    Task<RatingOptionResult?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IEnumerable<RatingOptionResult>> GetForGroupIdAsync(Guid? groupId, CancellationToken ct);

    Task<IEnumerable<RatingOptionResult>> GetDefaultsAsync(CancellationToken ct);

    Task<bool> IsGroupUsingDefaultValues(Guid groupId, CancellationToken ct);

    Task<Guid> AddAsync(SetOptionRequest request, CancellationToken ct);

    Task<IEnumerable<Guid>> AddRangeAsync(SetOptionsRequest request, CancellationToken ct);

    Task RemoveCustomRatingsForGroupAsync(Guid groupId, CancellationToken ct);

    Task UpdateAsync(Guid optionId, string label, CancellationToken ct);

    Task DeleteAsync(Guid optionId, CancellationToken ct);

    Task CondenseDisplayOrdersAsync(Guid groupId, Guid deletedOptionId, CancellationToken ct);

    Task ReorderAsync(Guid groupId, Guid[] orderedOptionIds, CancellationToken ct);
}
