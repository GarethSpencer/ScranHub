using DAL.Entities.Abstractions;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions.Generic;

public interface ITypeOptionRepository<TTypeOption> : IEFRepository<TTypeOption>
    where TTypeOption : class, ITypeOption
{
    Task<IEnumerable<TypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
}
