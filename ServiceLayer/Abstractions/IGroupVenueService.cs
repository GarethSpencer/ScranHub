using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface IGroupVenueService
{
    Task<CommonResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct);

    Task<CommonResponse> SearchGroupVenuesAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct);

    Task<CommonResponse> CreateGroupVenueAsync(CreateGroupVenueRequest request, CancellationToken ct);

    Task<CommonResponse> UpdateGroupVenueAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct);

    Task<CommonResponse> DeleteGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
}