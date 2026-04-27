using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IVenueTypeOptionRepository : IEFRepository<VenueTypeOption>
    {
        Task<VenueTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<VenueTypeOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}