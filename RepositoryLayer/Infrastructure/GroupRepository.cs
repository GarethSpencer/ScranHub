using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;
using Utilities.Models.Requests.Groups;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupRepository(ScranHubDbContext dbContext) : EFRepository<Group>(dbContext), IGroupRepository
{
    public async Task<GroupResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([id], ct);

        if (group == null)
        {
            return null;
        }

        return new GroupResult
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            Active = group.Active,
            CreatedBy = group.CreatedBy
        };
    }

    public async Task<GroupResult?> GetByNameAsync(string name, CancellationToken ct)
    {
        var group = await _dbSet.FirstOrDefaultAsync(x => x.GroupName == name, ct);

        if (group == null)
        {
            return null;
        }

        return new GroupResult
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            Active = group.Active,
            CreatedBy = group.CreatedBy
        };
    }

    public async Task<(IEnumerable<GroupResult>, int)> SearchByNameAsync(SearchGroupRequest request, CancellationToken ct)
    {
        var groupsQuery = _dbSet.Where(x => EF.Functions.Like(x.GroupName, $"%{request.SearchText}%"));
        
        //TODO filter by friend, admin

        var totalCount = await groupsQuery.CountAsync(ct);

        var groups = await groupsQuery
            .OrderBy(x => x.GroupName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var groupResults = groups.Select(g => new GroupResult
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            Active = g.Active,
            CreatedBy = g.CreatedBy
        });

        return (groupResults, totalCount);
    }

    public async Task<Guid> CreateAsync(string groupName, CancellationToken ct)
    {
        var group = new Group
        {
            GroupName = groupName,
            Active = true
        };

        await _dbSet.AddAsync(group, ct);
        return group.GroupId;
    }

    public async Task DeactivateAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);

        group?.Active = false;
    }

    public async Task DeleteAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);
        if (group != null)
        {
            _dbSet.Remove(group);
        }
    }

    public async Task<bool> DidUserCreateGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(ug => ug.GroupId == groupId && ug.CreatedBy == userId, ct);
    }

    public async Task UpdateAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);
        if (group != null)
        {
            group.GroupName = groupRequest.GroupName;
            group.Active = groupRequest.Active;
        }
    }
}
