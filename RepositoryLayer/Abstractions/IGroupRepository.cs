using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupRepository : IEFRepository<Group>
    {
        Task<IEnumerable<Group>> GetAllActiveGroupsAsync(CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<Group?> GetByNameAsync(string name, CancellationToken ct, bool trackChanges = false);
        Task<Guid> CreateGroup(GroupRequest request, CancellationToken ct);
    }
}