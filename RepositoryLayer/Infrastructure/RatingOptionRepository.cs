using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class RatingOptionRepository(ScranHubDbContext dbContext) : EFRepository<RatingOption>(dbContext), IRatingOptionRepository
{
    public async Task<RatingOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<RatingOption>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.RatingOptionId == id, ct);
    }

    public async Task<IEnumerable<RatingOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<RatingOption> query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }
}