using System.ComponentModel;

namespace Utilities.Models.Requests.Authentication;

public record AuthenticationDataRequest
{
    [DefaultValue("test")]
    public string? UserName { get; set; }

    [DefaultValue("Password123!")]
    public string? Password { get; set; }
}
