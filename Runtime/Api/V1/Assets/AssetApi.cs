using System.Threading.Tasks;

namespace Instasounds.Api
{
    public class AssetApi : BaseApi
    {
        public const string Resource = "assets";

        private readonly InstasoundsSettings _settings;

        public AssetApi(InstasoundsSettings settings) : base(settings)
        {
            _settings = settings;
        }

        public virtual async Task<AssetListResponse> ListAssetsAsync(AssetListRequest request, RequestCallbacks callbacks = null)
        {
            var queryString = request.Params.GenerateQueryString();

            return await GetAsync<AssetListResponse>(new Request()
            {
                Url = $"{_settings.ApiBaseUrl}/v1/{Resource}{queryString}",
                Callbacks = callbacks
            });
        }
    }
}