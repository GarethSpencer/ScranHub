using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class UserGroupRepository(ScranHubDbContext dbContext) : EFRepository<UserGroup>(dbContext), IUserGroupRepository
{
    public async Task<Guid> AddUserToGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        var userGroup = new UserGroup
        {
            GroupId = groupId,
            UserId = userId
        };
        await _dbSet.AddAsync(userGroup, ct);
        return userGroup.UserGroupId;
    }

    public async Task<IEnumerable<GroupResult>> GetGroupsForUserAsync(Guid userId, CancellationToken ct)
    {
        var query = _dbSet.Where(ug => ug.UserId == userId);

        var userGroups = await query
            .Select(ug => new GroupResult
            {
                GroupId = ug.GroupId,
                GroupName = ug.Group!.GroupName,
                Active = ug.Group.Active,
                CreatedBy = ug.Group.CreatedBy,
                CreatedOn = ug.Group.CreatedOn
            })
            .ToListAsync(ct);

        return userGroups;
    }

    public async Task<bool> IsUserInGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId, ct);
    }

    public async Task<int> GetGroupMemberCountAsync(Guid groupId, CancellationToken ct)
    {
        return await _dbSet.CountAsync(ug => ug.GroupId == groupId, ct);
    }

    public async Task RemoveUserFromGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        var userGroup = await _dbSet.FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId, ct);
        if (userGroup != null)
        {
            _dbSet.Remove(userGroup);
        }
    }
}
