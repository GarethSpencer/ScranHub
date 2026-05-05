namespace Utilities.Models.Requests.Users;

public record UpdateUserRequest
{
    public required string DisplayName { get; set; }
    public required bool Admin { get; set; }
    public required bool Active { get; set; }
}
