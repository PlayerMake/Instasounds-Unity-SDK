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

        public static async Task<User> VerifyApiKeyAsync(string apiKey, RequestCallbacks callbacks = null)
        {
            Init();

            var userResponse = await _userApi.VerifyApiKeyAsync(apiKey, callbacks);

            if (userResponse.IsSuccess)
                return userResponse.Data;

            return null;
        }

        public static async Task<(List<Asset>, Pagination)> ListAssetsAsync(
            string searchTerm = "",
            string selectedSort = "",
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
