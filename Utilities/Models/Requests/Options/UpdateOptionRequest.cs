namespace Utilities.Models.Requests.Options;

public record UpdateOptionRequest
{
    public required string Label { get; set; }
}
