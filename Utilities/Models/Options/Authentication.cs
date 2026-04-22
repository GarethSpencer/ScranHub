namespace Utilities.Models.Options;

public class Authentication
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
