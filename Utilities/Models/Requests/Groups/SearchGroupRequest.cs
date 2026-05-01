using Utilities.Models.Requests.Generic;

namespace Utilities.Models.Requests.Groups;

public record SearchGroupRequest : PaginationBaseRequest
{
    public required string SearchText { get; set; }
}
