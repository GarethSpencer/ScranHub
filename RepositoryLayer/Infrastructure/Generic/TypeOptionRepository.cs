using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;
using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public abstract class TypeOptionRepository<TTypeOption>(ScranHubDbContext dbContext) : EFRepository<TTypeOption>(dbContext), ITypeOptionRepository
    where TTypeOption : class, ITypeOption, new()
{
    public async Task<IEnumerable<TypeOptionResult>> GetForGroupIdAsync(Guid? groupId, CancellationToken ct)
    {
        var options = await _dbSet
            .Where(x => x.GroupId == groupId)
            .Select(ProjectToResult())
            .OrderBy(x => x.Label)
            .ToListAsync(ct);

        if (options.Count == 0)
        {
            options = await _dbSet
                .Where(x => x.GroupId == null)
                .Select(ProjectToResult())
                .OrderBy(x => x.Label)
                .ToListAsync(ct);
        }

        return options;
    }

    public async Task RemoveCustomTypesForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var optionsToRemove = await _dbSet
            .Where(x => x.GroupId == groupId)
            .ToListAsync(ct);

        if (optionsToRemove.Count > 0)
        {
            _dbSet.RemoveRange(optionsToRemove);
        }
    }

    public async Task<IEnumerable<TypeOptionResult>> GetDefaultsAsync(CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.GroupId == null)
            .OrderBy(x => x.Label)
            .Select(ProjectToResult())
            .ToListAsync(ct);
    }

    public async Task<bool> IsGroupUsingDefaultValues(Guid groupId, CancellationToken ct)
    {
        var count = await _dbSet
            .Where(x => x.GroupId == groupId)
            .CountAsync(ct);
        return count == 0;
    }

    public async Task<IEnumerable<Guid>> AddRangeAsync(SetOptionsRequest request, CancellationToken ct)
    {
        var optionsToAdd = new List<TTypeOption>();
        foreach (var label in request.Labels)
        {
            optionsToAdd.Add(new TTypeOption
            {
                GroupId = request.GroupId,
                Label = label
            });
        }

        await _dbSet.AddRangeAsync(optionsToAdd, ct);
        return optionsToAdd.Select(x => x.OptionId);
    }

    public async Task<Guid> AddAsync(SetOptionRequest request, CancellationToken ct)
    {
        var optionToAdd = new TTypeOption
        {
            GroupId = request.GroupId,
            Label = request.Label
        };

        await _dbSet.AddAsync(optionToAdd, ct);
        return optionToAdd.OptionId;
    }

    public abstract Task<TypeOptionResult?> GetByIdAsync(Guid id, CancellationToken ct);

    public abstract Task UpdateAsync(Guid optionId, string label, CancellationToken ct);

    public abstract Task DeleteAsync(Guid optionId, CancellationToken ct);

    protected static Expression<Func<TTypeOption, TypeOptionResult>> ProjectToResult() =>
    x => new TypeOptionResult
    {
        OptionId = x.OptionId,
        GroupId = x.GroupId,
        Label = x.Label
    };
}
