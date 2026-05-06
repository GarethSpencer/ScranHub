using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class FoodTypeOptionRepository(ScranHubDbContext dbContext) : EFRepository<FoodTypeOption>(dbContext), IFoodTypeOptionRepository
{
    public async Task<FoodTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbSet.FindAsync([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.FoodTypeOptionId == id, ct);
    }

    public async Task<IEnumerable<FoodTypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!query.Any()) {
            var defaultOptions = await _dbSet.Where(x => x.GroupId == null).OrderBy(x => x.DisplayOrder).ToListAsync(ct);
            return defaultOptions.Select(x => new FoodTypeOptionResult
            {
                FoodTypeOptionId = x.FoodTypeOptionId,
                GroupId = x.GroupId,
                Label = x.Label,
                DisplayOrder = x.DisplayOrder
            });
        }

        return await query.Select(x => new FoodTypeOptionResult
        {
            FoodTypeOptionId = x.FoodTypeOptionId,
            GroupId = x.GroupId,
            Label = x.Label,
            DisplayOrder = x.DisplayOrder
        }).ToListAsync(ct);
    }
}