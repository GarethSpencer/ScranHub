using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface ITypeOptionRepository
{
    Task<TypeOptionResult?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IEnumerable<TypeOptionResult>> GetForGroupIdAsync(Guid? groupId, CancellationToken ct);

    Task<IEnumerable<TypeOptionResult>> GetDefaultsAsync(CancellationToken ct);

    Task<bool> IsGroupUsingDefaultValues(Guid groupId, CancellationToken ct);

    Task<Guid> AddAsync(SetOptionRequest request, CancellationToken ct);

    Task<IEnumerable<Guid>> AddRangeAsync(SetOptionsRequest request, CancellationToken ct);

    Task RemoveCustomTypesForGroupAsync(Guid groupId, CancellationToken ct);

    Task UpdateAsync(Guid optionId, string label, CancellationToken ct);

    Task DeleteAsync(Guid optionId, CancellationToken ct);
}
