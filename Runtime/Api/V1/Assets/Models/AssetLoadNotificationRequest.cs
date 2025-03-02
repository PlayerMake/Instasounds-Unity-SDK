using Newtonsoft.Json;

public class AssetLoadNotificationRequest
{
    public AssetLoadNotificationBody Body { get; set; } = new AssetLoadNotificationBody();
}

public class AssetLoadNotificationBody
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("cached")]
    public bool Cached { get; set; }
}
