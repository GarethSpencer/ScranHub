using System.Linq.Expressions;

namespace RepositoryLayer.Abstractions.Generic;

public interface IEFRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    void Delete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, int? skip = null, int? take = null, CancellationToken ct = default);
    void Update(T entity);
}