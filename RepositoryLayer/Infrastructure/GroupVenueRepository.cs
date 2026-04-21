using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupVenueRepository(ScranHubDbContext dbContext) : EFRepository<GroupVenue>(dbContext), IGroupVenueRepository
{
    public async Task<GroupVenue?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<GroupVenue>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.GroupVenueId == id, ct);
    }

    public async Task<IEnumerable<GroupVenue>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<GroupVenue> query = _dbSet.Where(x => x.GroupId == groupId)
            .Include(x => x.Group)
            .Include(x => x.CostUserRatings)!.ThenInclude(x => x.CostOption)
            .Include(x => x.FoodTypeOption)
            .Include(x => x.RatingUserRatings)!.ThenInclude(x => x.RatingOption)
            .Include(x => x.VenueTypeOption);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }
}