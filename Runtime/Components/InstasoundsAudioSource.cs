using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class InstasoundsAudioSource : MonoBehaviour
{
    public string audioUrl = "https://playermake-permanent-files.s3.eu-west-2.amazonaws.com/audio/baboon_monkey.wav";

    public AudioSource audioSource;

    public bool playOnLoad;

    void Start()
    {
        StartCoroutine(DownloadAndPlayAudio(audioUrl));
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
