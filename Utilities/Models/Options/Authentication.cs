namespace Utilities.Models.Options;

public record Authentication
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}
