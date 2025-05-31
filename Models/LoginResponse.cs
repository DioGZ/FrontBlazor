using System.Text.Json.Serialization;

namespace FrontBlazor.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
