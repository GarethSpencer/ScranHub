using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class QualityOptionRepository(ScranHubDbContext dbContext) : EFRepository<QualityOption>(dbContext), IQualityOptionRepository
{
    public async Task<QualityOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbSet.FindAsync([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.QualityOptionId == id, ct);
    }

    public async Task<IEnumerable<QualityOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<QualityOption> query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }
}