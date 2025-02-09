using RuntimeSounds.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RuntimeSounds.V1
{
    public static class RuntimeSoundsSdk
    {
        private static RuntimeSoundsSettings _developerSettings;
        private static AssetApi _assetApi;

        private static void Init()
        {
            if (_developerSettings == null)
                _developerSettings = Resources.Load<RuntimeSoundsSettings>("RuntimeSoundsSettings");

            if (_assetApi == null)
                _assetApi = new AssetApi(_developerSettings);
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

        public static async Task<AudioClip> LoadAudioClipAsync(string url)
        {
            Init();

            return await FileApi.DownloadAudioAsync(url);
        }
    }
}
