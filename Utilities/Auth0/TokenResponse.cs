using System.Text.Json.Serialization;

namespace Utilities.Auth0;

public sealed partial class Auth0UserService
{
    private sealed record TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    }
}