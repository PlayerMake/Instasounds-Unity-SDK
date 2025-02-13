using Newtonsoft.Json;
using RuntimeSounds.Api;

public class UserGetResponse : Response
{
    [JsonProperty("data")]
    public User Data { get; set; }
}