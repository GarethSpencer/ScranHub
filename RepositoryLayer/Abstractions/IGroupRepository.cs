using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllActiveGroupsAsync(CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByNameAsync(string name, CancellationToken ct, bool trackChanges = false);
    }
}