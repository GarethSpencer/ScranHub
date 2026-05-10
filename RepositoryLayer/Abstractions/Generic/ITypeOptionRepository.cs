using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface ITypeOptionRepository
{
    Task<IEnumerable<TypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
}
