using Workshop.Web.Models;

public interface IApiAuthStrategy
{
    Task<APIAuthorization> GetAuthorizationAsync();
    void SetApiSettings(ApiSettings apiSettings);
    void SetHttpClient(HttpClient httpClient);
}