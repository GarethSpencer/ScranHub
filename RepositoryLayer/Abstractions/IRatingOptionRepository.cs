using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IRatingOptionRepository : IEFRepository<RatingOption>
    {
        Task<RatingOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<RatingOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}