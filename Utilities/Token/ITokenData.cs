namespace Utilities.Token;

public interface ITokenData
{
    Guid? UserId { get; set; }
    string? AuthId { get; set; }
    bool IsAdmin { get; set; }
}
