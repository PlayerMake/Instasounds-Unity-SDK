using UnityEngine;
using UnityEditor;
using System;
using RuntimeSounds.Editor.UI.Windows;
using RuntimeSounds.Api;
using RuntimeSounds.V1;

[CustomEditor(typeof(RuntimeAudio))]
public class RuntimeAudioEditor : Editor
{
    public class TempAudioData
    {
        public bool Playing;

        public EditorApplication.CallbackFunction clipendCallback;

        public AudioSource audioSource;

        public float CurrentTime;
    }

    public TempAudioData selectedClipData = new TempAudioData();

    private GameObject previewGameObject;


    void OnEnable()
    {
        if (!previewGameObject)
        {
            previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("PreviewAudio", HideFlags.HideAndDontSave);
        }
    }

    private EditorApplication.CallbackFunction updateCallback;
    private EditorApplication.CallbackFunction repaintCallback;

    private double nextRepaintTime = 0;

    public static string FormatTime(float totalSeconds)
    {
        var time = TimeSpan.FromSeconds(totalSeconds);
        
        return string.Format("{0:00}:{1:00}:{2:00}", time.Minutes, time.Seconds, Math.Round((double)time.Milliseconds / 10f));
    }


    public override void OnInspectorGUI()
    {
        var myTarget = (RuntimeAudio)target;

            // Load the custom icon from the Resources folder
        var icon = Resources.Load<Texture2D>("RuntimeSound");

            // Assign the icon to the script asset
        var script = MonoScript.FromMonoBehaviour((RuntimeAudio)target);
        if (script != null && icon != null)
        {
            EditorGUIUtility.SetIconForObject(script, icon);
            AssetDatabase.SaveAssets();
        }

        if (repaintCallback == null)
        {
            repaintCallback = () =>
            {
                if (EditorApplication.timeSinceStartup > nextRepaintTime)
                {
                    nextRepaintTime = EditorApplication.timeSinceStartup + 0.05;

                    Repaint();
                }
            };

            EditorApplication.update += repaintCallback;
        }

       // EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Audio Clip", new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Normal
        });

        if (myTarget.selectedAsset != null && !string.IsNullOrEmpty(myTarget.selectedAsset.Url))
        {
            RenderAudioClip(myTarget.selectedAsset, selectedClipData);
        }
        else
        {
            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 10)
            });

            EditorGUILayout.BeginHorizontal();

            // Display the audio clip name

            EditorGUILayout.BeginVertical();

            Rect waveformRect = GUILayoutUtility.GetRect(100, 10, new GUIStyle()
            {
                margin = new RectOffset(0, 0, 7, 0)
            });
            GUI.DrawTexture(waveformRect, new Texture2D(200, 10));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("None", GUILayout.Width(70));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("00:00:00", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Play Button
            GUI.enabled = false;
            if (GUILayout.Button("▶ Play", GUILayout.Width(60), GUILayout.Height(34)))
            {
            }
            GUI.enabled = true;

            if (GUILayout.Button("Set", GUILayout.Width(60), GUILayout.Height(34)))
            {
                AudioSearchWindow.Open(this, myTarget);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.LabelField("Audio Settings", new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.white
            }
        });

        myTarget.playOnLoad = EditorGUILayout.Toggle("Play on Load", myTarget.playOnLoad);
        myTarget.audioSource = (AudioSource)EditorGUILayout.ObjectField("Audio Source", myTarget.audioSource, typeof(AudioSource), true);
        EditorGUILayout.LabelField("Note that the AudioClip property of the Audio Source component set above will be overriden by this component.", new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = true,
        });

        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(myTarget);
        }
    }

    private void RenderAudioClip (Asset asset, TempAudioData clipData)
    {
        EditorGUILayout.BeginVertical(new GUIStyle()
        {
            margin = new RectOffset(0, 0, 0, 10)
        });
        EditorGUILayout.BeginHorizontal();

        // Display the audio clip name

        EditorGUILayout.BeginVertical();

        Rect waveformRect = GUILayoutUtility.GetRect(100, 10, new GUIStyle()
        {
            margin = new RectOffset(0, 0, 7, 0)
        });
        GUI.DrawTexture(waveformRect, new Texture2D(200, 10));

        if (clipData.audioSource != null && clipData.audioSource.isPlaying)
        {
            clipData.CurrentTime = clipData.audioSource.time;
            float playbackX = waveformRect.x + GetPlaybackPosition(clipData.audioSource) * waveformRect.width;
            Handles.color = Color.red;
            Handles.DrawLine(new Vector3(playbackX, waveformRect.y), new Vector3(playbackX, waveformRect.y + waveformRect.height));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(asset.Name, GUILayout.Width(70));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(FormatTime(clipData.CurrentTime), GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        //if (clip.Playing)
        //    GUI.enabled = false;

        // Play Button
        if (GUILayout.Button(clipData.Playing ? "Stop" : "▶ Play", GUILayout.Width(60), GUILayout.Height(34)))
        {
            if (clipData.Playing)
            {
                clipData.Playing = false;
                clipData.audioSource.Stop();
                clipData.CurrentTime = 0;
                DestroyImmediate(clipData.audioSource);
                clipData.audioSource = null;
                EditorApplication.update -= clipData.clipendCallback;
            }
            else
            {
                clipData.Playing = true;

                RuntimeSoundsSdk
                    .LoadAudioClipAsync(asset.Url)
                    .ContinueWith(p =>
                    {
                        updateCallback = () => PlayClipOnMainThread(p.Result, clipData); 

                        EditorApplication.update += updateCallback;
                    });
            }

        }

        if (GUILayout.Button("Change", GUILayout.Width(60), GUILayout.Height(34)))
        {
            var myTarget = (RuntimeAudio)target;

            /* if (selectedClip != null)
            {
                selectedClip.Playing = false;
                selectedClip.audioSource?.Stop();
                selectedClip.CurrentTime = 0;
                if (selectedClip.audioSource != null)
                    DestroyImmediate(selectedClip.audioSource);
                selectedClip.audioSource = null;
                selectedClip = null;
                myTarget.selectedAsset = null;
            } */

            AudioSearchWindow.Open(this,myTarget);
        }
        //GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        /* if (EditorGUI.EndChangeCheck())
        {
            // Mark object as dirty to ensure changes are saved
            EditorUtility.SetDirty(MonoScript.FromMonoBehaviour((RuntimeAudio)target));
        } */
    }

    private Action ListenForClipFinish(double stopTime, TempAudioData asset)
    {
         if (EditorApplication.timeSinceStartup >= stopTime)
         {
            asset.Playing = false;
            DestroyImmediate(asset.audioSource);
            asset.audioSource = null;
            EditorApplication.update -= asset.clipendCallback;
         }

        return () => { };
    }

    private Action PlayClipOnMainThread(AudioClip clip, TempAudioData audioMetadata)
    {
        // This is a closure to ensure the flag is handled for one-time execution.
        bool played = false;

        if (played)
        {
            EditorApplication.update -= updateCallback;
        }
        else
        {
            played = true;
            PlayClip(clip, audioMetadata);

            // Schedule an action after clip.length seconds
            double stopTime = EditorApplication.timeSinceStartup + clip.length;

            audioMetadata.clipendCallback = () => ListenForClipFinish(stopTime, audioMetadata);

            EditorApplication.update += audioMetadata.clipendCallback;

            EditorApplication.update -= updateCallback; // Remove the callback
        }

        return () => { };
    }


    private void PlayClip(AudioClip clip, TempAudioData audioMetadata)
    {
        if (audioMetadata.audioSource == null)
        {
            audioMetadata.audioSource = previewGameObject.AddComponent<AudioSource>();
        }

        audioMetadata.audioSource.Stop();
        audioMetadata.audioSource.clip = clip;
        audioMetadata.audioSource.volume = 1f; // Ensure volume is set
        audioMetadata.audioSource.mute = false; // Ensure it's not muted
        audioMetadata.audioSource.Play();
    }

    void OnDisable()
    {
        if (previewGameObject) DestroyImmediate(previewGameObject.gameObject);

        AudioSearchWindow.ForceClose();
    }


    private float GetPlaybackPosition(AudioSource audioSource)
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.time / audioSource.clip.length; // Normalize between 0 and 1
    }
}