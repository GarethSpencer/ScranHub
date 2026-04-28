using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IUserGroupRepository : IEFRepository<UserGroup>
    {
        Task<Guid> AddUserToGroup(Guid groupId, Guid userId, CancellationToken ct);

        Task<IEnumerable<UserGroupResult>> GetGroupsForUser(Guid userId, CancellationToken ct, bool trackChanges = false);
    }
}