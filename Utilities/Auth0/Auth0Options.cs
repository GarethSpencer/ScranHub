using System.ComponentModel.DataAnnotations;

namespace Utilities.Auth0;

public sealed class Auth0Options
{
    [Required] public string Domain { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
    [Required] public string ClientSecret { get; set; } = string.Empty;
}
