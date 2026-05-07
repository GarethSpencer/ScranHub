using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IQualityOptionRepository : IEFRepository<QualityOption>
    {
        Task<QualityOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<QualityOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}