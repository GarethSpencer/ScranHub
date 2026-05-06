using System.Linq.Expressions;

namespace RepositoryLayer.Abstractions.Generic;

public interface IEFRepository<T> where T : class
{
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}