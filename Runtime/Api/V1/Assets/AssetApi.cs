using System.Collections.Generic;
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
                Callbacks = callbacks,
                Headers = new Dictionary<string, string>()
                {
                    { "x-api-key", _settings.ApiKey }
                }
            });
        }

        public virtual async Task<AssetGenerationResponse> GenerateAssetAsync(AssetGenerationRequest request, RequestCallbacks callbacks = null)
        {
            return await PostAsync<AssetGenerationResponse, AssetGenerationBody>(new RequestWithBody<AssetGenerationBody>()
            {
                Url = $"{_settings.ApiBaseUrl}/v1/{Resource}/generate",
                Payload = request.Body,
                Callbacks = callbacks,
                Headers = new Dictionary<string, string>()
                {
                    { "x-api-key", _settings.ApiKey },
                    { "Content-Type", "application/json" }
                }
            });
        }

        public virtual async Task<AssetImportResponse> ImportAssetAsync(AssetImportRequest request, RequestCallbacks callbacks = null)
        {
            return await PostAsync<AssetImportResponse, AssetImportBody>(new RequestWithBody<AssetImportBody>()
            {
                Url = $"{_settings.ApiBaseUrl}/v1/{Resource}/cache",
                Payload = request.Body,
                Callbacks = callbacks,
                Headers = new Dictionary<string, string>()
                {
                    { "x-api-key", _settings.ApiKey },
                    { "Content-Type", "application/json" }
                }
            });
        }

        public virtual async Task<AssetLoadNotificationResponse> NotifyOfLoadAsync(AssetLoadNotificationRequest request, RequestCallbacks callbacks = null)
        {
            return await PostAsync<AssetLoadNotificationResponse, AssetLoadNotificationBody>(new RequestWithBody<AssetLoadNotificationBody>()
            {
                Url = $"{_settings.ApiBaseUrl}/v1/{Resource}/notify",
                Payload = request.Body,
                Callbacks = callbacks,
                Headers = new Dictionary<string, string>()
                {
                    { "x-api-key", _settings.ApiKey },
                    { "Content-Type", "application/json" }
                }
            });
        }
    }
}