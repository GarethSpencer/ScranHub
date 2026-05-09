using DAL.Data;
using DAL.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure.Generic;

public class TypeOptionRepository<TTypeOption>(ScranHubDbContext dbContext) : EFRepository<TTypeOption>(dbContext), ITypeOptionRepository<TTypeOption>
    where TTypeOption : class, ITypeOption
{
    public async Task<IEnumerable<TypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
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

    private static Expression<Func<TTypeOption, TypeOptionResult>> ProjectToResult() =>
    x => new TypeOptionResult
    {
        TypeOptionId = x.TypeOptionId,
        GroupId = x.GroupId,
        Label = x.Label
    };
}
