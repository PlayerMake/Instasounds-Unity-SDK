using Newtonsoft.Json;

namespace RuntimeSounds.Api
{
    public class PaginationQueryParams
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("skip")]
        public int Skip { get; set; }
    }
}