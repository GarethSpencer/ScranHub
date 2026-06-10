namespace Utilities.Models.Results;

public record UserAuthResult
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string? AuthId { get; set; }
    public required bool Admin { get; init; }
    public required bool Active { get; init; }
}
