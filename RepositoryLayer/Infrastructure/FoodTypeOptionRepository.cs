using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class FoodTypeOptionRepository(ScranHubDbContext dbContext)
    : TypeOptionRepository<FoodTypeOption>(dbContext), IFoodTypeOptionRepository
{
    public override async Task<TypeOptionResult?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.FoodTypeOptionId == id)
            .Select(ProjectToResult())
            .FirstOrDefaultAsync(ct);
    }

    public override async Task UpdateAsync(Guid optionId, string label, CancellationToken ct)
    {
        var optionToUpdate = await _dbSet.FirstOrDefaultAsync(x => x.FoodTypeOptionId == optionId, ct);
        optionToUpdate?.Label = label;
    }

    public override async Task DeleteAsync(Guid optionId, CancellationToken ct)
    {
        var optionToDelete = await _dbSet.FirstOrDefaultAsync(x => x.FoodTypeOptionId == optionId, ct);

        if (optionToDelete != null)
        {
            _dbSet.Remove(optionToDelete);
        }
    }
}
