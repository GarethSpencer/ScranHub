namespace RepositoryLayer.Abstractions.Generic;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}