namespace Utilities.Models.Requests;

public record UpdateGroupRequest
{
    public required string GroupName { get; set; }
    public required bool Active { get; set; }
}
