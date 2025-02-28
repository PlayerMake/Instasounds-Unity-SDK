using Newtonsoft.Json;

public class AssetGenerationRequest
{
    public AssetGenerationBody Body { get; set; } = new AssetGenerationBody();
}

public class AssetGenerationBody
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("prompt")]
    public string Prompt { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }
}
