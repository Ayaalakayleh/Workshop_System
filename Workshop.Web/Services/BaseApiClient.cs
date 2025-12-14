using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class BaseApiClient
    {

        protected readonly HttpClient _httpClient;
        protected readonly ApiSettings _apiSettings;
        protected readonly IApiAuthStrategy _apiAuthStrategy;

        protected readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BaseApiClient(
            HttpClient httpClient,
            ApiSettings apiSettings,
            IApiAuthStrategy apiAuthStrategy
            )
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings;
            _apiAuthStrategy = apiAuthStrategy;

            _apiAuthStrategy.SetHttpClient(_httpClient);
            _apiAuthStrategy.SetApiSettings(_apiSettings);
        }


        protected async Task<T?> SendRequest<T>(string url, HttpMethod httpMethod, object? body = null, bool requireAuth = true)
        {
            var request = new HttpRequestMessage(httpMethod, _httpClient.BaseAddress + url);

            if (requireAuth)
            {
                APIAuthorization auth = await _apiAuthStrategy.GetAuthorizationAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.AccessToken);
            }

            if (body != null && (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Patch || httpMethod == HttpMethod.Delete))
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();


            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                var wrapper = JsonSerializer.Deserialize<Response>(responseString, _jsonOptions);
                if (wrapper != null && wrapper.IsScusses && wrapper.ResponseDetails is JsonElement element)
                {
                    return element.Deserialize<T>(_jsonOptions);
                }
            }
            catch (JsonException) { 
            
            }

            return JsonSerializer.Deserialize<T>(responseString, _jsonOptions);


        }

        protected async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string requestUri)
        {
            var auth = await _apiAuthStrategy.GetAuthorizationAsync();
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.AccessToken);
            return request;
        }


    }
}
