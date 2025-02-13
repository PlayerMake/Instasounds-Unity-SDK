using Newtonsoft.Json;
using System;

namespace RuntimeSounds.Api
{
    [Serializable]
    public class User
    {
        [JsonProperty("_id")]
        public string Id;

        [JsonProperty("tier")]
        public string Tier;
    }
}
