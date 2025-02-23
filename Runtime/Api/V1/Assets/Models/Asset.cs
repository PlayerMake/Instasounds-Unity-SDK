using Newtonsoft.Json;
using System;

namespace RuntimeSounds.Api
{
    [Serializable]
    public class Asset
    {
        [JsonProperty("_id")]
        public string Id;

        [JsonProperty("externalId")]
        public string ExternalId;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("aiGenerated")]
        public bool AiGenerated;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("previewUrl")]
        public string PreviewUrl;

        [JsonProperty("iconUrl")]
        public string IconUrl;

        [JsonProperty("isPremium")]
        public bool IsPremium;
    }
}
