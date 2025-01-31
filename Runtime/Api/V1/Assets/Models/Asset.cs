using Newtonsoft.Json;
using System;

namespace Instasounds.Api
{
    [Serializable]
    public class Asset
    {
        [JsonProperty("_id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("iconUrl")]
        public string IconUrl;
    }
}
