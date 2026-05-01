using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupRepository : IEFRepository<Group>
    {
        Task<GroupResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct);

        Task<GroupResult?> GetByNameAsync(string name, CancellationToken ct);

        Task<(IEnumerable<GroupResult>, int)> SearchByNameAsync(SearchGroupRequest request, CancellationToken ct);

        Task<Guid> CreateAsync(string groupName, CancellationToken ct);

        Task DeleteAsync(Guid groupId, CancellationToken ct);

        Task<bool> DidUserCreateGroupAsync(Guid groupId, Guid userId, CancellationToken ct);

        Task DeactivateAsync(Guid groupId, CancellationToken ct);

        Task UpdateAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct);
    }
}