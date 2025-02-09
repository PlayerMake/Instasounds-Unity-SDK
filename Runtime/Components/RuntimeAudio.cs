using RuntimeSounds.Api;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RuntimeAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public Asset selectedAsset;

    public bool playOnLoad;

    void Start()
    {
        if (selectedAsset != null && !string.IsNullOrEmpty(selectedAsset.Url))
            StartCoroutine(DownloadAndPlayAudio(selectedAsset.Url));
    }

    IEnumerator DownloadAndPlayAudio(string url)
    {
        using (var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading audio: " + request.error);
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;

                if (playOnLoad) 
                    audioSource.Play();
            }
        }
    }
}
