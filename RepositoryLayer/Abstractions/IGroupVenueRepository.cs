using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupVenueRepository : IEFRepository<GroupVenue>
    {
        Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, CancellationToken ct);
        Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct);
    }
}
