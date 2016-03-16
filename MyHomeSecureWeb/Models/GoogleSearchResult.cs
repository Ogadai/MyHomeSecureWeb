using Newtonsoft.Json;

namespace MyHomeSecureWeb.Models
{
    public class GoogleSearchResult
    {
        [JsonProperty("items")]
        public GoogleSearchItem[] Items { get; set; }
    }

    public class GoogleSearchItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
