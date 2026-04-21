using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class VenueTypeOptionRepository(ScranHubDbContext dbContext) : EFRepository<VenueTypeOption>(dbContext), IVenueTypeOptionRepository
{
    public async Task<VenueTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<VenueTypeOption>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.VenueTypeOptionId == id, ct);
    }

    public async Task<IEnumerable<VenueTypeOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<VenueTypeOption> query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }
}