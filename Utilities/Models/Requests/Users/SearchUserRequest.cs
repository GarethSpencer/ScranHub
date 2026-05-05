using Utilities.Models.Requests.Generic;

namespace Utilities.Models.Requests.Users;

public record SearchUserRequest : PaginationBaseRequest
{
    public required string SearchText { get; set; }
}
