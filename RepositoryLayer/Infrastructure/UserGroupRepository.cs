using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.Generic;
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
                CreatedOn = ug.Group.CreatedOn,
                DisplayName = ug.Group.CreatedByUser.DisplayName,
                UserCount = ug.Group.UserGroups.Count,
                VenueCount = ug.Group.GroupVenues.Count
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

    public async Task<(IEnumerable<UserResult>, int)> GetMembersByIdAsync(Guid groupId, PaginationBaseRequest request, CancellationToken ct)
    {
        var usersQuery = _dbSet
            .Where(x => x.GroupId == groupId);

        var users = usersQuery.Select(g => new UserResult
        {
            UserId = g.User!.UserId,
            DisplayName = g.User.DisplayName,
            Active = g.User.Active,
        });

        var count = await users.CountAsync(ct);

        var output = await users.OrderBy(x => x.DisplayName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (output, count);
    }
}
