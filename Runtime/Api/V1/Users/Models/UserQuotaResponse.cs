using Newtonsoft.Json;
using RuntimeSounds.Api;

public class UserQuotaResponse : Response
{
    [JsonProperty("data")]
    public UserQuota Data { get; set; }
}
