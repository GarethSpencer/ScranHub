using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class VenueTypeOptionRepository(ScranHubDbContext dbContext) : EFRepository<VenueTypeOption>(dbContext), IVenueTypeOptionRepository
{
    public async Task<VenueTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbSet.FindAsync([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.VenueTypeOptionId == id, ct);
    }

    public async Task<IEnumerable<VenueTypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!query.Any())
        {
            var defaultOptions = await _dbSet.Where(x => x.GroupId == null).OrderBy(x => x.DisplayOrder).ToListAsync(ct);
            return defaultOptions.Select(x => new VenueTypeOptionResult
            {
                VenueTypeOptionId = x.VenueTypeOptionId,
                GroupId = x.GroupId,
                Label = x.Label,
                DisplayOrder = x.DisplayOrder
            });
        }

        return await query.Select(x => new VenueTypeOptionResult
        {
            VenueTypeOptionId = x.VenueTypeOptionId,
            GroupId = x.GroupId,
            Label = x.Label,
            DisplayOrder = x.DisplayOrder
        }).ToListAsync(ct);
    }
}