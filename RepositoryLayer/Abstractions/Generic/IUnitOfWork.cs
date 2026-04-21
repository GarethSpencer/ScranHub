namespace RepositoryLayer.Abstractions.Generic;

public interface IUnitOfWork
{
    Task<int> SaveChanges(CancellationToken ct = default);
}