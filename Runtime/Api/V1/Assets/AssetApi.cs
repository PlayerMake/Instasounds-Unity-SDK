using System.Threading.Tasks;

namespace RuntimeSounds.Api
{
    public class AssetApi : BaseApi
    {
        public const string Resource = "assets";

        private readonly RuntimeSoundsSettings _settings;

        public AssetApi(RuntimeSoundsSettings settings) : base(settings)
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