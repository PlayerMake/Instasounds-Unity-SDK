using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Instasounds.V1;
using System;
using Instasounds.Api;
using System.Linq;

[CustomEditor(typeof(InstasoundsAudioSource))] // Links this editor to CustomComponent
public class InstasoundsComponentEditor : Editor
{
    class LocalAsset : Asset
    {
        public bool Playing;

        public EditorApplication.CallbackFunction clipendCallback;

        public AudioSource audioSource;

        public float CurrentTime;
    }

    private string searchText = "";
    private bool searchOpen = true;
    private LocalAsset selectedClip = null;

    private List<LocalAsset> foundClips = new List<LocalAsset>()
    {
        new LocalAsset()
        {
            Url = "https://playermake-permanent-files.s3.eu-west-2.amazonaws.com/audio/baboon_monkey.wav",
            Name = "Monkey",
            Id = "Test",
            Playing = false,
        },
        new LocalAsset()
        {
            Url = "https://playermake-permanent-files.s3.eu-west-2.amazonaws.com/audio/baboon_monkey.wav",
            Name = "Monkey 2",
            Id = "Test1",
            Playing = false,
        },
        new LocalAsset()
        {
            Url = "https://playermake-permanent-files.s3.eu-west-2.amazonaws.com/audio/baboon_monkey.wav",
            Name = "Monkey 3",
            Id = "Test2",
            Playing = false,
        },
    };

    private GameObject previewGameObject;


    void OnEnable()
    {
        if (!previewGameObject)
        {
            previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("PreviewAudio", HideFlags.HideAndDontSave, typeof(GameObject)); ;
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
        InstasoundsAudioSource myTarget = (InstasoundsAudioSource)target;

        if (repaintCallback == null)
        {
            repaintCallback = () =>
            {
                if (EditorApplication.timeSinceStartup > nextRepaintTime)
                {
                    nextRepaintTime = EditorApplication.timeSinceStartup + 0.05
                    ;
                    Repaint();
                }
            };

            EditorApplication.update += repaintCallback;
        }

        EditorGUILayout.LabelField("Selected Audio Clip", new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Normal
        });

        if (selectedClip != null)
            RenderAudioClip(selectedClip);
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

            if (GUILayout.Button("Change", GUILayout.Width(60), GUILayout.Height(34)))
            {
            }
            GUI.enabled = true;

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


        searchOpen = EditorGUILayout.Foldout(searchOpen, "Change Audio Clip", new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            },
            active = new GUIStyleState()
            {
                textColor = Color.white
            },
            onNormal = new GUIStyleState()
            {
                textColor = Color.white
            }
        });


        if (searchOpen)
        {
            EditorGUILayout.LabelField("Search Audio Clips:");

            searchText = EditorGUILayout.TextField("Name", searchText);

            foreach (var clip in foundClips)
            {
                RenderAudioClip(clip);
            }
        }


        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(myTarget);
        }
    }

    private void RenderAudioClip (LocalAsset clip)
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

        if (clip.audioSource != null && clip.audioSource.isPlaying)
        {
            clip.CurrentTime = clip.audioSource.time;
            float playbackX = waveformRect.x + GetPlaybackPosition(clip.audioSource) * waveformRect.width;
            Handles.color = Color.red;
            Handles.DrawLine(new Vector3(playbackX, waveformRect.y), new Vector3(playbackX, waveformRect.y + waveformRect.height));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(clip.Name, GUILayout.Width(70));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(FormatTime(clip.CurrentTime), GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        //if (clip.Playing)
        //    GUI.enabled = false;

        // Play Button
        if (GUILayout.Button(clip.Playing ? "Stop" : "▶ Play", GUILayout.Width(60), GUILayout.Height(34)))
        {
            if (clip.Playing)
            {
                clip.Playing = false;
                clip.audioSource.Stop();
                clip.CurrentTime = 0;
                DestroyImmediate(clip.audioSource);
                clip.audioSource = null;
                EditorApplication.update -= clip.clipendCallback;
            }
            else
            {
                clip.Playing = true;

                InstasoundsSdk
                    .LoadAudioClipAsync(clip.Url)
                    .ContinueWith(p =>
                    {
                        updateCallback = () => PlayClipOnMainThread(p.Result, clip);

                        EditorApplication.update += updateCallback;
                    });
            }

        }

        if (GUILayout.Button(selectedClip?.Id == clip.Id ? "Unselect" : "Select", GUILayout.Width(60), GUILayout.Height(34)))
        {
            if (selectedClip?.Id == clip.Id)
            {
                selectedClip = null;
            }
            else
            {
                selectedClip = clip;
            }
        }
        //GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private Action ListenForClipFinish(double stopTime, LocalAsset asset)
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

    private Action PlayClipOnMainThread(AudioClip clip, LocalAsset asset)
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
            PlayClip(clip, asset);

            // Schedule an action after clip.length seconds
            double stopTime = EditorApplication.timeSinceStartup + clip.length;

            asset.clipendCallback = () => ListenForClipFinish(stopTime, asset);

            EditorApplication.update += asset.clipendCallback;

            EditorApplication.update -= updateCallback; // Remove the callback
        }

        return () => { };
    }


    private void PlayClip(AudioClip clip, LocalAsset asset)
    {
        if (asset.audioSource == null)
        {
            asset.audioSource = previewGameObject.AddComponent<AudioSource>();
        }

        asset.audioSource.Stop();
        asset.audioSource.clip = clip;
        asset.audioSource.volume = 1f; // Ensure volume is set
        asset.audioSource.mute = false; // Ensure it's not muted
        asset.audioSource.Play();
    }

    void OnDisable()
    {
        if (previewGameObject) DestroyImmediate(previewGameObject.gameObject);
    }

    private float GetPlaybackPosition(AudioSource audioSource)
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.time / audioSource.clip.length; // Normalize between 0 and 1
    }
}