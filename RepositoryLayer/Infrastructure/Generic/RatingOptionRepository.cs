using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public class RatingOptionRepository<TRatingOption>(ScranHubDbContext dbContext) : EFRepository<TRatingOption>(dbContext), IRatingOptionRepository<TRatingOption>
    where TRatingOption : class, IRatingOption
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

    private static Expression<Func<TRatingOption, RatingOptionResult>> ProjectToResult() =>
    x => new RatingOptionResult
    {
        OptionId = x.OptionId,
        GroupId = x.GroupId,
        Label = x.Label,
        DisplayOrder = x.DisplayOrder
    };
}
