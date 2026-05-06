using Utilities.Models.Requests.Generic;

namespace Utilities.Models.Requests.GroupVenues;

public record SearchGroupVenueRequest : PaginationBaseRequest
{
    public string? SearchText { get; set; } = null;
}
