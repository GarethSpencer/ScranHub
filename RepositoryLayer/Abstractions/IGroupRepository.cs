using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupRepository : IEFRepository<Group>
    {
        Task<IEnumerable<Group>> GetAllActiveGroupsAsync(CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByNameAsync(string name, CancellationToken ct, bool trackChanges = false);
        Task<Guid> CreateGroup(string groupName, CancellationToken ct);
    }
}