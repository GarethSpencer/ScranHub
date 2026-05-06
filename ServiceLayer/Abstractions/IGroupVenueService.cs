using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.GroupVenues;

namespace ServiceLayer.Abstractions;

public interface IGroupVenueService
{
    Task<GetGroupVenueResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct);

    Task<AddGroupVenueResponse> CreateGroupVenueAsync(CreateGroupVenueRequest request, CancellationToken ct);
}