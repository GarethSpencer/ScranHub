namespace Utilities.Models.Results;

public record GroupResult
{
    public required Guid GroupId { get; init; }
    public required string GroupName { get; init; }
    public required bool Active { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime CreatedOn { get; init; }
}
