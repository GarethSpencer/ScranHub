using Utilities.Models.Responses.GroupVenue;

namespace ServiceLayer.Abstractions;

public interface IGroupVenueService
{
    Task<GetGroupVenueResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct);
}