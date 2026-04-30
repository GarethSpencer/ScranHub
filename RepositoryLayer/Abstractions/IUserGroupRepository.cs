using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IUserGroupRepository : IEFRepository<UserGroup>
    {
        Task<Guid> AddUserToGroupAsync(Guid groupId, Guid userId, CancellationToken ct);

        Task<IEnumerable<GroupResult>> GetGroupsForUserAsync(Guid userId, CancellationToken ct);

        Task<bool> IsUserInGroupAsync(Guid groupId, Guid userId, CancellationToken ct);

        Task<int> GetGroupMemberCountAsync(Guid groupId, CancellationToken ct);

        Task RemoveUserFromGroupAsync(Guid groupId, Guid userId, CancellationToken ct);
    }
}