using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface IGroupVenueRepository : IEFRepository<GroupVenue>
{
    Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, CancellationToken ct);
    Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct);
    Task<Guid> CreateAsync(CreateGroupVenueRequest request, CancellationToken ct);
    Task UpdateAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct);
    Task DeleteAsync(Guid groupVenueId, CancellationToken ct);
    Task<(IEnumerable<GroupVenueResult>, int)> GetByGroupIdAsync(Guid groupId, PaginationBaseRequest request, CancellationToken ct);
    Task<(IEnumerable<GroupVenueResult>, int)> SearchByNameAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct);
}
