namespace Utilities.Models.Results;

public record UserGroupResult
{
    public required Guid GroupId { get; init; }
    public required string GroupName { get; init; }
    public required bool Active { get; init; }
    public required bool CreatedByUser { get; init; }
}
