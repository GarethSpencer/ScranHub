namespace Utilities.Models.Requests.Options;

public record OrderOptionsRequest
{
    public required Guid GroupId { get; set; }
    public required Guid[] OptionsIds { get; set; }
}
