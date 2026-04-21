using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface ICostOptionRepository
    {
        Task<CostOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<CostOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}