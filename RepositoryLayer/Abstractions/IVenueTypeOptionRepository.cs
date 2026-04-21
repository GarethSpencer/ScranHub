using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IVenueTypeOptionRepository
    {
        Task<VenueTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<VenueTypeOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}