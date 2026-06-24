using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface IGroupVenueRepository : IEFRepository<GroupVenue>
{
    Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, Guid callingUserId, CancellationToken ct);
    Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, Guid callingUserId, CancellationToken ct);
    Task<Guid> CreateAsync(CreateGroupVenueRequest request, CancellationToken ct);
    Task UpdateAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct);
    Task DeleteAsync(Guid groupVenueId, CancellationToken ct);
    Task<(IEnumerable<GroupVenueResult>, int)> GetByGroupIdAsync(Guid groupId, SortableGroupVenueRequest request, Guid callingUserId, CancellationToken ct);
    Task<(IEnumerable<GroupVenueResult>, int)> SearchByNameAsync(Guid groupId, SearchGroupVenueRequest request, Guid callingUserId, CancellationToken ct);
}
