using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;
using Utilities.Models.Requests.Options;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public class RatingOptionRepository<TRatingOption>(ScranHubDbContext dbContext) : EFRepository<TRatingOption>(dbContext), IRatingOptionRepository
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

    private static Expression<Func<TRatingOption, RatingOptionResult>> ProjectToResult() =>
    x => new RatingOptionResult
    {
        OptionId = x.OptionId,
        GroupId = x.GroupId,
        Label = x.Label,
        DisplayOrder = x.DisplayOrder
    };
}
