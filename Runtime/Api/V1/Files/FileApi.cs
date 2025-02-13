using System;
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
                    Debug.LogError("You have hit the limit on your account. Please go to https://runtimesounds.com/ to upgrade, if you are getting value from our plugin and want to include more audio in your game.");
                }
                else
                {
                    Debug.LogError("Unexpected error downloading audio: " + webRequest.error);
                }
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(webRequest);
        }
    }
}
