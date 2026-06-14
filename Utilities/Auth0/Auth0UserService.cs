using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Utilities.Auth0;

public sealed partial class Auth0UserService(
    HttpClient httpClient,
    IOptions<Auth0Options> options,
    ILogger<Auth0UserService> logger) : IAuth0UserService
{
    private readonly Auth0Options _options = options.Value;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private readonly ILogger _logger = logger;

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;


    public async Task DeleteUserAsync(string authId, CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v2/users/{Uri.EscapeDataString(authId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Auth0 user {AuthId} already absent; treating as deleted", authId);
            }
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt)
            {
                return _cachedToken;
            }

            var payload = new
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret,
                audience = $"https://{_options.Domain}/api/v2/",
                grant_type = "client_credentials"
            };

            using var response = await httpClient.PostAsJsonAsync("oauth/token", payload, ct);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
                ?? throw new InvalidOperationException("Auth0 token response was empty");

            _cachedToken = token.AccessToken;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 60);
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}