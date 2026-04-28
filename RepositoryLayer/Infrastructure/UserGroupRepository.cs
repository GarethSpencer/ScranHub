using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class UserGroupRepository(ScranHubDbContext dbContext) : EFRepository<UserGroup>(dbContext), IUserGroupRepository
{
    public async Task<Guid> AddUserToGroup(Guid groupId, Guid userId, CancellationToken ct)
    {
        var userGroup = new UserGroup
        {
            GroupId = groupId,
            UserId = userId
        };
        await _dbSet.AddAsync(userGroup, ct);
        return userGroup.UserGroupId;
    }

    public async Task<IEnumerable<UserGroupResult>> GetGroupsForUser(Guid userId, CancellationToken ct, bool trackChanges = false)
    {
        var query = _dbSet.Where(ug => ug.UserId == userId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        var userGroups = await query
            .Select(ug => new UserGroupResult
            {
                GroupId = ug.GroupId,
                GroupName = ug.Group!.GroupName
            })
            .ToListAsync(ct);

        return userGroups;
    }
}
