using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupVenueRepository
    {
        Task<IEnumerable<GroupVenue>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
        Task<GroupVenue?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
    }
}