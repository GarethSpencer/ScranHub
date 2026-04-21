using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IRatingOptionRepository
    {
        Task<RatingOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<RatingOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}