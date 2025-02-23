using Newtonsoft.Json;

namespace RuntimeSounds.Api
{
    public class AssetImportRequest
    {
        public AssetImportBody Body { get; set; } = new AssetImportBody();
    }

    public class AssetImportBody
    {
        [JsonProperty("sourceId")]
        public string SourceId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}