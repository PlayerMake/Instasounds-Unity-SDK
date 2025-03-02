using Newtonsoft.Json;
using RuntimeSounds.Api;

public class AssetLoadNotificationResponse : Response
{
    [JsonProperty("completed")]
    public bool Completed { get; set; }
}
