namespace Utilities.Token;

public class TokenData : ITokenData
{
    public Guid? UserId { get; set; }
    public string? AuthId { get; set; }
    public bool IsAdmin { get; set; }
}
