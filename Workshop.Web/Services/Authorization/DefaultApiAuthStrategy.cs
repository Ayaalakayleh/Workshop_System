using Microsoft.Extensions.Options;
using System.Text.Json;
using Workshop.Web.Models;

public class DefaultApiAuthStrategy : IApiAuthStrategy
{
    private HttpClient _httpClient;
    private ApiSettings _apiSettings;
    private APIAuthorization? _cachedToken;
    private DateTime _tokenExpiry;


    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DefaultApiAuthStrategy()
    {
    }


    public async Task<APIAuthorization> GetAuthorizationAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        var content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("username", _apiSettings.ApiUser),
                new KeyValuePair<string, string>("password", _apiSettings.ApiPassword),
                new KeyValuePair<string, string>("grant_type", "password")
            });

        var response = await _httpClient.PostAsync("/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Authentication failed: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var authorization = JsonSerializer.Deserialize<APIAuthorization>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize APIAuthorization.");

        if (string.IsNullOrWhiteSpace(authorization.AccessToken) || string.IsNullOrWhiteSpace(authorization.TokenType))
        {
            throw new InvalidOperationException($"Invalid token received. Response: {responseContent}");
        }

        _cachedToken = authorization;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(authorization.ExpiresIn - 60); // 1-minute buffer

        return authorization;
    }

    public void SetApiSettings(ApiSettings apiSettings)
    {
        _apiSettings = apiSettings;
    }

    public void SetHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}