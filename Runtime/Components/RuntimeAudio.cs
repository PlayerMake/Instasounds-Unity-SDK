using RuntimeSounds.Api;
using RuntimeSounds.V1;
using System.Collections;
using UnityEngine;

public class RuntimeAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public Asset selectedAsset;

    public bool playOnLoad;

    void Start()
    {
        if (selectedAsset != null && !string.IsNullOrEmpty(selectedAsset.Url))
            StartCoroutine(DownloadAndPlayAudio(selectedAsset.Url, selectedAsset.Id));
    }

    IEnumerator DownloadAndPlayAudio(string url, string assetId)
    {
        var audioClip = AudioCacheReader.Load(assetId);

        if (audioClip != null)
        {
            RuntimeSoundsSdk.NotifyOfLoad(assetId, true);

            audioSource.clip = audioClip;

            if (playOnLoad)
                audioSource.Play();

            yield break;
        }

        var downloadTask = RuntimeSoundsSdk.LoadAudioClipAsync(url);

        while (!downloadTask.IsCompleted)
            yield return null;

        if (downloadTask.IsCompletedSuccessfully)
        {
            audioSource.clip = downloadTask.Result;

            if (playOnLoad)
                audioSource.Play();
        }
    }
}
