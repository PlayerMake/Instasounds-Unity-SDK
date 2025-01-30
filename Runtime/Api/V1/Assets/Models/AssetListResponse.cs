using Newtonsoft.Json;
using System.Collections.Generic;

namespace Instasounds.Api
{
    public class AssetListResponse : Response
    {
        [JsonProperty("data")]
        public List<Asset> Data { get; set; } = new List<Asset>();

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
