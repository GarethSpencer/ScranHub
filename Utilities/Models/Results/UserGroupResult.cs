namespace Utilities.Models.Results;

public record UserGroupResult
{
    public required Guid GroupId { get; init; }
    public required string GroupName { get; init; }
    public required int Users { get; init; }
}
