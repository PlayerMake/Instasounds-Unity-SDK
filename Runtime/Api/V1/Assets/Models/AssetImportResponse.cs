using Newtonsoft.Json;
using RuntimeSounds.Api;

public class AssetImportResponse : Response
{
    [JsonProperty("data")]
    public Asset Data { get; set; }
}
