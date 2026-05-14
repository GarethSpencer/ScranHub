using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class CostOptionRepository(ScranHubDbContext dbContext)
    : RatingOptionRepository<CostOption>(dbContext), ICostOptionRepository
{
    public override async Task<RatingOptionResult?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.CostOptionId == id)
            .Select(ProjectToResult())
            .FirstOrDefaultAsync(ct);
    }

    public override async Task UpdateAsync(Guid optionId, string label, CancellationToken ct)
    {
        var optionToUpdate = await _dbSet.FirstOrDefaultAsync(x => x.CostOptionId == optionId, ct);
        optionToUpdate?.Label = label;
    }

    public override async Task DeleteAsync(Guid optionId, CancellationToken ct)
    {
        var optionToDelete = await _dbSet.FirstOrDefaultAsync(x => x.CostOptionId == optionId, ct);

        if (optionToDelete != null)
        {
            _dbSet.Remove(optionToDelete);
        }
    }

    public override async Task CondenseDisplayOrdersAsync(Guid groupId, Guid deletedOptionId, CancellationToken ct)
    {
        var options = await _dbSet
            .Where(x => x.GroupId == groupId && x.CostOptionId != deletedOptionId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

        for (var i = 0; i < options.Count; i++)
        {
            options[i].DisplayOrder = i + 1;
        }
    }
}
