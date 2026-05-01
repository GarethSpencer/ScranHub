namespace Utilities.Models.Results;

public record UserResult
{
    public required Guid UserId { get; init; }
    public required Guid? AuthId { get; init; }
    public required string DisplayName { get; init; }
    public required bool Active { get; init; }
    public required bool Admin { get; init; }
    public required string Email { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime CreatedOn { get; init; }
}
