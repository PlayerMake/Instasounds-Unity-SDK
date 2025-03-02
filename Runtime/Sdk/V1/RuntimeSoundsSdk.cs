using RuntimeSounds.Api;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RuntimeSounds.V1
{
    public static class RuntimeSoundsSdk
    {
        private static RuntimeSoundsSettings _developerSettings;
        private static AssetApi _assetApi;
        private static UserApi _userApi;

        private static void Init()
        {
            if (_developerSettings == null)
                _developerSettings = Resources.Load<RuntimeSoundsSettings>("RuntimeSoundsSettings");

            if (_assetApi == null)
                _assetApi = new AssetApi(_developerSettings);

            if (_userApi == null)
                _userApi = new UserApi(_developerSettings);
        }


        public static void NotifyOfLoad(string assetId, bool cached)
        {
            Init();

            _ = _assetApi.NotifyOfLoadAsync(new AssetLoadNotificationRequest()
            {
                Body = new AssetLoadNotificationBody()
                {
                    Id = assetId,
                    Cached = cached,
                }
            });
        }

        public static async Task<User> VerifyApiKeyAsync(string apiKey, RequestCallbacks callbacks = null)
        {
            Init();

            var userResponse = await _userApi.VerifyApiKeyAsync(apiKey, callbacks);

            if (userResponse.IsSuccess)
                return userResponse.Data;

            return null;
        }

        public static async Task<UserQuota> GetQuotaAsync(string apiKey, RequestCallbacks callbacks = null)
        {
            Init();

            var quotaResponse = await _userApi.GetQuoataAsync(apiKey, callbacks);

            if (quotaResponse.IsSuccess)
                return quotaResponse.Data;

            return null;
        }

        public static async Task<Asset> GenerateAssetAsync(
            string description,
            string name,
            int duration,
            RequestCallbacks callbacks = null
            )
        {
            Init();

            var generationResponse = await _assetApi.GenerateAssetAsync(new AssetGenerationRequest()
            {
                Body = new AssetGenerationBody()
                {
                    Duration = duration,
                    Prompt = description,
                    Name = name
                }
            }, callbacks);

            return generationResponse.Data;
        }

        public static async Task<Asset> ImportAssetAsync(
            string source,
            string id,
            RequestCallbacks callbacks = null
            )
        {
            Init();

            var importResponse = await _assetApi.ImportAssetAsync(new AssetImportRequest()
            {
                Body = new AssetImportBody()
                {
                    Source = source,
                    SourceId = id,
                }
            }, callbacks);

            return importResponse.Data;
        }

        public static async Task<(List<Asset>, Pagination)> ListAssetsAsync(
            string searchTerm = "",
            string selectedSort = "",
            string selectedTier = "",
            string selectedSource = "",
            int limit = 10,
            int skip = 0,
            RequestCallbacks callbacks = null
            )
        {
            Init();

            var assetResponse = await _assetApi.ListAssetsAsync(new AssetListRequest()
            {
                Params = new AssetListQueryParams()
                {
                    Sort = selectedSort,
                    Search = searchTerm,
                    Tier = selectedTier,
                    Source = selectedSource,
                    Limit = limit,
                    Skip = skip
                }
            }, callbacks);

            return (assetResponse.Data, assetResponse.Pagination);
        }

        public static async Task<byte[]> DownloadFileIntoMemoryAsync(string url, CancellationToken cancellationToken = default)
        {
            return await FileApi.DownloadFileIntoMemoryAsync(url, _developerSettings.ApiKey, cancellationToken);
        }

        public static async Task<AudioClip> LoadAudioClipAsync(string url)
        {
            Init();

            return await FileApi.DownloadAudioAsync(url, _developerSettings.ApiKey);
        }
    }
}
