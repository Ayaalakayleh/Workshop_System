using System.Text;
using Workshop.Web.Models;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;

public class OldApiAuthStrategy : IApiAuthStrategy
{
    private HttpClient _httpClient;
    private ApiSettings _apiSettings;

    public OldApiAuthStrategy()
    { }

    private readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
        }
    };

    public async Task<APIAuthorization> GetAuthorizationAsync()
    {
        try
        {
            string url = $"{_httpClient.BaseAddress.AbsoluteUri}/token";
            Console.WriteLine($"username={_apiSettings.ApiUser}&password={_apiSettings.ApiPassword}&grant_type=password");
            var bytes = Encoding.ASCII.GetBytes($"username={_apiSettings.ApiUser}&password={_apiSettings.ApiPassword}&grant_type=password");
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = (long)bytes.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var s = streamReader.ReadToEnd();
                        return JsonConvert.DeserializeObject<APIAuthorization>(s, _jsonSettings);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            throw new HttpRequestException("Authentication failed", ex);
        }
    }

    public void SetApiSettings(ApiSettings apiSettings)
    {
        _apiSettings = new ApiSettings { ApiUser = apiSettings.ApiUser, ApiPassword = apiSettings.ApiPassword };
    }

    public void SetHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
