using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KeePassXC_API
{
    [DebuggerDisplay("{Title}")]
    public class AccountInformation
    {
        [JsonPropertyName("name")]
        public string Title { get; set; }
        [JsonPropertyName("group")]
        public string Group { get; set; }
        [JsonPropertyName("login")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
