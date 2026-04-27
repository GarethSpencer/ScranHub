using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupVenueRepository : IEFRepository<GroupVenue>
    {
        Task<IEnumerable<GroupVenue>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
        Task<GroupVenue?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
    }
}