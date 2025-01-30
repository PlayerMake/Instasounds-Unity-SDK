using Instasounds.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Instasounds.V1
{
    public static class InstasoundsSdk
    {
        private static InstasoundsSettings _developerSettings;
        private static AssetApi _assetApi;

        private static void Init()
        {
            if (_developerSettings == null)
                _developerSettings = Resources.Load<InstasoundsSettings>("PlayerMakeSettings");

            if (_assetApi == null)
                _assetApi = new AssetApi(_developerSettings);
        }

        public static async Task<(List<Asset>, Pagination)> ListAssetsAsync(
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
                    ProjectId = _developerSettings.ProjectId,
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
