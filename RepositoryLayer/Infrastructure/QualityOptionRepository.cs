using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class QualityOptionRepository(ScranHubDbContext dbContext) : EFRepository<QualityOption>(dbContext), IQualityOptionRepository
{
    public async Task<IEnumerable<QualityOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.GroupId == groupId).OrderBy(x => x.DisplayOrder);

        if (!query.Any())
        {
            var defaultOptions = await _dbSet.Where(x => x.GroupId == null).OrderBy(x => x.DisplayOrder).ToListAsync(ct);
            return defaultOptions.Select(x => new QualityOptionResult
            {
                QualityOptionId = x.QualityOptionId,
                GroupId = x.GroupId,
                Label = x.Label,
                DisplayOrder = x.DisplayOrder
            });
        }

        return await query.Select(x => new QualityOptionResult
        {
            QualityOptionId = x.QualityOptionId,
            GroupId = x.GroupId,
            Label = x.Label,
            DisplayOrder = x.DisplayOrder
        }).ToListAsync(ct);
    }
}