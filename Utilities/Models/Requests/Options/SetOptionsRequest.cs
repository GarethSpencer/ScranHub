namespace Utilities.Models.Requests.Options;

public record SetOptionsRequest
{
    public required Guid GroupId { get; set; }
    public required string[] Labels { get; set; }
}
