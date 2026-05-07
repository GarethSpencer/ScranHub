using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IQualityOptionRepository : IEFRepository<QualityOption>
    {
        Task<IEnumerable<QualityOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
    }
}