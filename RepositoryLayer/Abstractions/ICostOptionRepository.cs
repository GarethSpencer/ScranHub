using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface ICostOptionRepository : IEFRepository<CostOption>
    {
        Task<CostOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<CostOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}