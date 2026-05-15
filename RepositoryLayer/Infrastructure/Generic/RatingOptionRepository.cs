using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;
using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public abstract class RatingOptionRepository<TRatingOption>(ScranHubDbContext dbContext) : EFRepository<TRatingOption>(dbContext), IRatingOptionRepository
    where TRatingOption : class, IRatingOption, new()
{
    public async Task<IEnumerable<RatingOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
    {
        var options = await _dbSet
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.DisplayOrder)
            .Select(ProjectToResult())
            .ToListAsync(ct);

        if (options.Count == 0)
        {
            options = await _dbSet
                .Where(x => x.GroupId == null)
                .OrderBy(x => x.DisplayOrder)
                .Select(ProjectToResult())
                .ToListAsync(ct);
        }

        return options;
    }

    public async Task RemoveCustomRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var optionsToRemove = await _dbSet
            .Where(x => x.GroupId == groupId)
            .ToListAsync(ct);

        if (optionsToRemove.Count > 0)
        {
            _dbSet.RemoveRange(optionsToRemove);
        }
    }

    public async Task<IEnumerable<RatingOptionResult>> GetDefaultsAsync(CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.GroupId == null)
            .OrderBy(x => x.DisplayOrder)
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
        var displayOrder = 1;
        var optionsToAdd = new List<TRatingOption>();
        foreach (var label in request.Labels)
        {
            optionsToAdd.Add(new TRatingOption
            {
                GroupId = request.GroupId,
                Label = label,
                DisplayOrder = displayOrder
            });
            displayOrder++;
        }

        await _dbSet.AddRangeAsync(optionsToAdd, ct);
        return optionsToAdd.Select(x => x.OptionId);
    }

    public async Task<Guid> AddAsync(SetOptionRequest request, CancellationToken ct)
    {
        var maxOptionDisplayOrder = await _dbSet.Where(x => x.GroupId == request.GroupId).OrderByDescending(x => x.DisplayOrder).FirstOrDefaultAsync(ct);
        var optionToAdd = new TRatingOption
        {
            GroupId = request.GroupId,
            DisplayOrder = maxOptionDisplayOrder!.DisplayOrder + 1,
            Label = request.Label
        };

        await _dbSet.AddAsync(optionToAdd, ct);
        return optionToAdd.OptionId;
    }

    public async Task ReorderAsync(Guid groupId, Guid[] orderedOptionIds, CancellationToken ct)
    {
        var options = await _dbSet
            .Where(x => x.GroupId == groupId)
            .ToListAsync(ct);

        for (var i = 0; i < orderedOptionIds.Length; i++)
        {
            var option = options.Single(x => x.OptionId == orderedOptionIds[i]);
            option.DisplayOrder = i + 1;
        }
    }

    public abstract Task CondenseDisplayOrdersAsync(Guid groupId, Guid deletedOptionId, CancellationToken ct);

    public abstract Task<RatingOptionResult?> GetByIdAsync(Guid id, CancellationToken ct);

    public abstract Task UpdateAsync(Guid optionId, string label, CancellationToken ct);

    public abstract Task DeleteAsync(Guid optionId, CancellationToken ct);

    protected static Expression<Func<TRatingOption, RatingOptionResult>> ProjectToResult() =>
    x => new RatingOptionResult
    {
        OptionId = x.OptionId,
        GroupId = x.GroupId,
        Label = x.Label,
        DisplayOrder = x.DisplayOrder
    };
}
