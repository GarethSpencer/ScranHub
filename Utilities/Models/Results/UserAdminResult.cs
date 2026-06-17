namespace Utilities.Models.Results;

public record UserAdminResult
{
    public required Guid UserId { get; init; }
    public required string? AuthId { get; init; }
    public required string DisplayName { get; init; }
    public required bool Active { get; init; }
    public required bool Admin { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required Guid? UpdatedBy { get; init; }
}
