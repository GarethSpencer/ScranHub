using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class CostOptionRepository(ScranHubDbContext dbContext) : EFRepository<CostOption>(dbContext), ICostOptionRepository
{
    public async Task<CostOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<CostOption>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.CostOptionId == id, ct);
    }

    public async Task<IEnumerable<CostOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<CostOption> query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }
}