using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupRepository(ScranHubDbContext dbContext) : EFRepository<Group>(dbContext), IGroupRepository
{
    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<Group>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.GroupId == id, ct);
    }

    public async Task<Group?> GetByNameAsync(string name, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<Group> query = _dbSet.Where(x => x.GroupName == name);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Group>> GetAllActiveGroupsAsync(CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<Group> query = _dbSet.Where(x => x.Active);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }

    public async Task<Guid> CreateGroup(GroupRequest request, CancellationToken ct)
    {
        var group = new Group
        {
            GroupId = Guid.NewGuid(),
            GroupName = request.GroupName,
            Active = true
        };
        await _dbSet.AddAsync(group, ct);
        return group.GroupId;
    }
}