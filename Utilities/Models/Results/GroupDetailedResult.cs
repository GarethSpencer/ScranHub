namespace Utilities.Models.Results;

public record GroupDetailedResult
{
    public required Guid GroupId { get; init; }
    public required string GroupName { get; init; }
    public required bool Active { get; init; }
    public required int UserCount { get; init; }
    public required int VenueCount { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required Guid? UpdatedBy { get; init; }
    public required string DisplayName { get; init; }
    public required string? Icon { get; init; }
}
