namespace Utilities.Models.Requests;

public record CreateGroupRequest
{
    public required string GroupName { get; set; }
}
