using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Instasounds.Api
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

        public static async Task<AudioClip> DownloadAudioAsync(string url)
        {
            var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            var asyncOperation = webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading audio: " + webRequest.error);
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(webRequest);
        }
    }
}
