using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RuntimeSounds.Api
{
    public static class FileApi
    {
        public static async Task<Texture2D> DownloadImageAsync(string url)
        {
            var webRequest = UnityWebRequestTexture.GetTexture(url);
            var asyncOperation = webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            return DownloadHandlerTexture.GetContent(webRequest);
        }

        public static async Task<AudioClip> DownloadAudioAsync(string url, string apiKey)
        {
            var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            webRequest.SetRequestHeader("x-api-key", apiKey);
            var asyncOperation = webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                if (webRequest.responseCode == 402)
                {
                    Debug.LogWarning("You have hit the limit on your account. Please go to https://runtimesounds.com/ to upgrade, if you are getting value from our plugin and want to include more audio in your game.");
                }
                else
                {
                    Debug.LogWarning("Error downloading audio. Please ensure your API Key is set in Tools -> Runtime Sounds -> Account");
                }
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(webRequest);
        }

        public static async Task<byte[]> DownloadFileIntoMemoryAsync(string url, string apiKey, CancellationToken cancellationToken = default)
        {
            using var request = new UnityWebRequest();
            request.url = url;
            request.SetRequestHeader("x-api-key", apiKey);
            request.downloadHandler = new DownloadHandlerBuffer();

            var asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning($"Request cancelled: {url}");
                    request.Abort();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download: " + request.error);
            }
            else
            {
                return request.downloadHandler.data;
            }

            return null;
        }
    }
}
