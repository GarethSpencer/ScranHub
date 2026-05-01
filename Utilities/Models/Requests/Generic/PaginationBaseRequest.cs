namespace Utilities.Models.Requests.Generic;

public record PaginationBaseRequest
{
    public required int PageNumber { get; set; } = 1;
    public required int PageSize { get; set; } = 10;
}
