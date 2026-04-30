namespace Utilities.Models.Requests.Groups;

public record SearchGroupRequest
{
    public required string SearchText { get; set; }
}
