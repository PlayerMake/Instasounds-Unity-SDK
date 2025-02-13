using RuntimeSounds.Api;
using RuntimeSounds.V1;
using System;
using UnityEditor;
using UnityEngine;

public class EditorAudioClip
{
    public bool playing;

    public bool loading;

    public EditorApplication.CallbackFunction clipendCallback;

    public AudioSource audioSource;

    public float currentTime;
}

public static class EditorAudio
{
    public static void PlayClip(AudioClip clip, EditorAudioClip asset, GameObject previewObject)
    {
        if (asset.audioSource == null)
        {
            asset.audioSource = previewObject.AddComponent<AudioSource>();
        }

        asset.audioSource.Stop();
        asset.audioSource.clip = clip;
        asset.audioSource.volume = 1f;
        asset.audioSource.mute = false;
        asset.audioSource.Play();
    }

    public static void DrawAudioClip(
        Asset asset,
        EditorAudioClip clipData,
        GameObject previewGameObject,
        EditorApplication.CallbackFunction updateCallback,
        bool usePreview = false
    ) {
        EditorGUILayout.BeginVertical();

        Rect waveformRect = GUILayoutUtility.GetRect(100, 10, new GUIStyle()
        {
            margin = new RectOffset(0, 0, 7, 0)
        });
        GUI.DrawTexture(waveformRect, new Texture2D(200, 10));

        if (clipData.audioSource != null && clipData.audioSource.isPlaying)
        {
            clipData.currentTime = clipData.audioSource.time;
            float playbackX = waveformRect.x + GetPlaybackPosition(clipData.audioSource) * waveformRect.width;
            Handles.color = Color.red;
            Handles.DrawLine(new Vector3(playbackX, waveformRect.y), new Vector3(playbackX, waveformRect.y + waveformRect.height));
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(asset.Name, GUILayout.Width(70));
        EditorGUILayout.Space();

        if (asset.IsPremium)
        {
            EditorGUILayout.LabelField("<color=orange>(BASIC TIER)</color> " + FormatTime(clipData.currentTime), new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleRight }, GUILayout.Width(133));
        } else
        {
            EditorGUILayout.LabelField(FormatTime(clipData.currentTime), new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
            }, GUILayout.Width(57));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (clipData.loading)
        {
            GUI.enabled = false;

            if (GUILayout.Button("Loading", GUILayout.Width(usePreview ? 74 : 60), GUILayout.Height(34)))
            {
            }

            GUI.enabled = true;
        }

        if (!clipData.loading)
        {
            if (!usePreview && String.IsNullOrEmpty(asset.Url))
                GUI.enabled = false;

            if (GUILayout.Button(clipData.playing ? "Stop" : (usePreview ? "▶ Preview" : "▶ Play"), GUILayout.Width(usePreview ? 74 : 60), GUILayout.Height(34)))
            {
                if (clipData.playing)
                {
                    clipData.playing = false;
                    clipData.audioSource.Stop();
                    clipData.currentTime = 0;
                    UnityEngine.Object.DestroyImmediate(clipData.audioSource);
                    clipData.audioSource = null;
                    EditorApplication.update -= clipData.clipendCallback;
                }
                else
                {
                    clipData.loading = true;

                    RuntimeSoundsSdk
                        .LoadAudioClipAsync(usePreview ? asset.PreviewUrl : asset.Url)
                        .ContinueWith(p =>
                        {
                            clipData.loading = false;

                            if (p.Result == null)

                            {
                                clipData.playing = false;
                                return;
                            }

                            clipData.playing = true;

                            updateCallback = () => PlayClipOnMainThread(p.Result, clipData, previewGameObject, updateCallback);

                            EditorApplication.update += updateCallback;
                        });
                }
            }
        }

        GUI.enabled = true;

    }

    public static float GetPlaybackPosition(AudioSource audioSource)
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.time / audioSource.clip.length;
    }
    public static string FormatTime(float totalSeconds)
    {
        var time = TimeSpan.FromSeconds(totalSeconds);

        return string.Format("{0:00}:{1:00}:{2:00}", time.Minutes, time.Seconds, Math.Round((double)time.Milliseconds / 10f));
    }

    public static Action PlayClipOnMainThread(
        AudioClip clip,
        EditorAudioClip asset,
        GameObject previewGameObject,
        EditorApplication.CallbackFunction updateCallback
    )
    {
        bool played = false;

        if (played)
        {
            EditorApplication.update -= updateCallback;
        }
        else
        {
            played = true;

            PlayClip(clip, asset, previewGameObject);

            var stopTime = EditorApplication.timeSinceStartup + clip.length;

            asset.clipendCallback = () => EditorAudio.CleanupOnClipFinish(stopTime, asset);

            EditorApplication.update += asset.clipendCallback;
            EditorApplication.update -= updateCallback;
        }

        return () => { };
    }

    public static Action CleanupOnClipFinish(double stopTime, EditorAudioClip asset)
    {
        if (EditorApplication.timeSinceStartup >= stopTime)
        {
            asset.playing = false;
            UnityEngine.Object.DestroyImmediate(asset.audioSource);
            asset.audioSource = null;
            EditorApplication.update -= asset.clipendCallback;
        }

        return () => { };
    }
}
