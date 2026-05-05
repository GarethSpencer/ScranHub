namespace Utilities.Models.Requests.Users;

public record AddFriendRequest
{
    public required string Email { get; set; }
}
