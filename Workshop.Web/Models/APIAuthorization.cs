using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Workshop.Web.Models
{
    public class APIAuthorization
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
