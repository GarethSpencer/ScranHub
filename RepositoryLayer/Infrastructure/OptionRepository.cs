using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using System.Linq.Expressions;
using Utilities.Models.Results.Abstractions;

namespace RepositoryLayer.Infrastructure;

public class OptionRepository<TOption, TOptionResult>(ScranHubDbContext dbContext) : EFRepository<TOption>(dbContext), IOptionRepository<TOption, TOptionResult>
    where TOption : class, IOption
    where TOptionResult : IOptionResult, new()
{
    public async Task<IEnumerable<TOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
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

    private static Expression<Func<TOption, TOptionResult>> ProjectToResult() =>
    x => new TOptionResult
    {
        OptionId = x.OptionId,
        GroupId = x.GroupId,
        Label = x.Label,
        DisplayOrder = x.DisplayOrder
    };
}
