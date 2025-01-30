using Newtonsoft.Json;

namespace Instasounds.Api
{
    public class Asset
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }
    }
}
