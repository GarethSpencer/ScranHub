using System.ComponentModel;

namespace Utilities.Models.Requests;

public record AuthenticationDataRequest
{
    [DefaultValue("test")]
    public string? UserName { get; init; }

    [DefaultValue("Password123!")]
    public string? Password { get; init; }
}
