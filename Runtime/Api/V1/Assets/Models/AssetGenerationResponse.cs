using Newtonsoft.Json;
using RuntimeSounds.Api;

public class AssetGenerationResponse : Response
{
    [JsonProperty("data")]
    public Asset Data { get; set; }
}
