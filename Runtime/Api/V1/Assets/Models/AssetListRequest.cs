using Newtonsoft.Json;

namespace RuntimeSounds.Api
{
    public class AssetListRequest
    {
        public AssetListQueryParams Params { get; set; } = new AssetListQueryParams();
    }

    public class AssetListQueryParams : PaginationQueryParams
    {
        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("sort")]
        public string Sort { get; set; }
    }
}