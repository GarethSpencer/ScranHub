using DAL.Entities.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results.Abstractions;

namespace RepositoryLayer.Abstractions;

public interface IOptionRepository<TOption, TOptionResult> : IEFRepository<TOption>
    where TOption : class, IOption
    where TOptionResult : IOptionResult, new()
{
    Task<IEnumerable<TOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
}
