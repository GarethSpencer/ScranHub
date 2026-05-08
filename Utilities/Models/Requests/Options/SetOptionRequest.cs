namespace Utilities.Models.Requests.Options;

public record SetOptionRequest
{
    public required string Label { get; set; }
}
