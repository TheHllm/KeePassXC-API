using System.Text.Json.Serialization;

namespace KeePassXC_API
{
    public class DatabaseInformation
    {
        [JsonPropertyName("key")]
        public string ClientIdentificationKey { get; set; }
        [JsonPropertyName("id")]
        public string ClientName { get; set; }

        public DatabaseInformation() : this(null, null) { }
        public DatabaseInformation(string key = null, string name = null) 
        {
            ClientIdentificationKey = key;
            ClientName = name;
        }
    }
}
