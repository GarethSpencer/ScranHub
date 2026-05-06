using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IGroupVenueRepository : IEFRepository<GroupVenue>
    {
        Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, CancellationToken ct);
        Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct);
        Task<Guid> CreateGroupVenue(CreateGroupVenueRequest request, CancellationToken ct);
        Task UpdateGroupVenueAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct);
    }
}
