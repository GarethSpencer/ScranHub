namespace Utilities.Models.Requests.Options;

public record SetOptionRequest
{
    public required Guid GroupId { get; set; }
    public required string Label { get; set; }
}
