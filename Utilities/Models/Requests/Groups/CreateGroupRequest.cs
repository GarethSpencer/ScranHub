namespace Utilities.Models.Requests.Groups;

public record CreateGroupRequest
{
    public required string GroupName { get; set; }
}
