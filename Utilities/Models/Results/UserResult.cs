namespace Utilities.Models.Results;

public record UserResult
{
    public required Guid UserId { get; init; }
    public required string? AuthId { get; set; }
    public required string DisplayName { get; init; }
    public required bool Active { get; init; }
    public required bool Admin { get; init; }
}
