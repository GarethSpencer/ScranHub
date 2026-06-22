using Utilities.Enums;
using Utilities.Models.Requests.Generic;

namespace Utilities.Models.Requests.GroupVenues;

public record SortableGroupVenueRequest : PaginationBaseRequest
{
    public required GroupVenueSortParameters SortBy { get; set; }
    public required bool SortDescending { get; set; } = false;
}