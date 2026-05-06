using DAL.Data;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions.Generic;
using System.Linq.Expressions;

namespace RepositoryLayer.Infrastructure.Generic;

public class EFRepository<T> : IEFRepository<T> where T : class
{
    protected readonly ScranHubDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public EFRepository(ScranHubDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate, ct);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate == null
            ? await _dbSet.AsNoTracking().CountAsync(ct)
            : await _dbSet.AsNoTracking().CountAsync(predicate, ct);
    }
}
