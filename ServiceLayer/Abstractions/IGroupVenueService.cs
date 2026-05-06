using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.GroupVenues;

namespace ServiceLayer.Abstractions;

public interface IGroupVenueService
{
    Task<GetGroupVenueResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct);

    Task<GetGroupVenuesResponse> SearchGroupVenuesAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct);

    Task<AddGroupVenueResponse> CreateGroupVenueAsync(CreateGroupVenueRequest request, CancellationToken ct);

    Task<CommonResponse> UpdateGroupVenueAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct);

    Task<CommonResponse> DeleteGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
}