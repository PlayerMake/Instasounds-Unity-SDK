using RuntimeSounds.Api;
using RuntimeSounds.Editor.UI.Components;
using RuntimeSounds.Editor.Utils;
using RuntimeSounds.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Windows
{
    public class AudioSearchWindow : EditorWindow
    {
        private static RuntimeAudio targetComponent;
        private static RuntimeAudioEditor _editorComponent;

        private static string searchQuery = "";
        private static int pageSize = 2;

        private Option[] sortOptions = null;
        private static string selectedSort = "relevance";

        private Debouncer debouncer;

        public class DownloadedData
        {
            public TempAudioData SceneData = new TempAudioData();

            public Asset Asset;
        }

        public class TempAudioData
        {
            public bool Playing;

            public EditorApplication.CallbackFunction clipendCallback;

            public AudioSource audioSource;

            public float CurrentTime;
        }

        private GameObject previewGameObject;

        private SelectInput sortSelectInput;
        private LoadingIndicator loadingIndicator;

        void OnEnable()
        {
            debouncer = new Debouncer(0.5f);

            if (!previewGameObject)
            {
                previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("PreviewAudio", HideFlags.HideAndDontSave);
            }

            if (sortSelectInput == null)
            {
                sortOptions = new List<Option>() {
                new Option()
                {
                    Label = "Relevance",
                    Value = "relevance"
                },
                new Option()
                {
                    Label = "Newest",
                    Value = "newest"
                },
                new Option()
                {
                    Label = "Oldest",
                    Value = "oldest"
                }
            }.ToArray();

                sortSelectInput = new SelectInput();
                sortSelectInput.Init(sortOptions, selectedSort);
            }

            if (loadingIndicator == null)
            {
                loadingIndicator = new LoadingIndicator();
            }
        }

        private void OnDestroy()
        {
            foreach (var clip in foundClips)
            {
                if (clip?.SceneData?.audioSource != null)
                    DestroyImmediate(clip.SceneData.audioSource);
            }

            EditorApplication.update -= repaintCallback;
        }

        private EditorApplication.CallbackFunction updateCallback;
        private EditorApplication.CallbackFunction repaintCallback;

        private double nextRepaintTime = 0;

        private static int totalItems = 0;
        private static int currentPage = 1;

        private static bool loading = false;

        private static bool forceClose = false;

        private static List<DownloadedData> foundClips = new List<DownloadedData>();

        private static void LoadItems(int page)
        {
            loading = true;

            try
            {
                RuntimeSoundsSdk.ListAssetsAsync(searchQuery, selectedSort, pageSize, (page - 1) * pageSize)
                    .ContinueWith(p =>
                    {
                        try
                        {
                            var response = p.Result;

                            totalItems = p.Result.Item2.Total;

                            currentPage = (p.Result.Item2.Skip / pageSize) + 1;

                            foundClips = response.Item1
                                .Select(asset => new DownloadedData()
                                {
                                    Asset = asset
                                })
                                .ToList();

                            loading = false;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            loading = false;
                        }
                    });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                loading = false;
            }
        }

        public static void ForceClose()
        {
            forceClose = true;
        }

        public static void Open(RuntimeAudioEditor editorComponent, RuntimeAudio component)
        {
            LoadItems(currentPage);

            // Create and show window
            var window = GetWindow<AudioSearchWindow>("Audio Picker");
            window.minSize = new Vector2(380, 300);

            // Pass the target component
            targetComponent = component;
            _editorComponent = editorComponent;
        }

        public static string FormatTime(float totalSeconds)
        {
            var time = TimeSpan.FromSeconds(totalSeconds);

            return string.Format("{0:00}:{1:00}:{2:00}", time.Minutes, time.Seconds, Math.Round((double)time.Milliseconds / 10f));
        }

        private void OnGUI()
        {
            if (forceClose == true)
            {
                forceClose = false;
                Close();
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


            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(10, 10, 6, 0)
            });

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Search", GUILayout.Width(50));
            var newSearchQuery = EditorGUILayout.TextField(searchQuery, GUILayout.ExpandWidth(true));

            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                debouncer.Execute(() => LoadItems(1));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 6, 0)
            });

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Sort", GUILayout.Width(50));

            sortSelectInput.Render((sort) =>
            {
                selectedSort = sort;
                LoadItems(0);

            }, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            EditorGUILayout.EndVertical();

            if (loading)
                loadingIndicator.Render(GUILayout.ExpandWidth(true));

            if (!loading)
            {
                EditorGUILayout.LabelField($"{totalItems} sound{(totalItems == 1 ? "" : "s")} found", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            EditorGUILayout.EndVertical();

            foreach (var clip in foundClips)
            {
                RenderAudioClip(clip.Asset, clip.SceneData);
            }

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            if (totalItems > pageSize)
            {
                EditorGUILayout.BeginHorizontal();

                if (currentPage == 1)
                    GUI.enabled = false;

                if (GUILayout.Button("Prev", GUILayout.Width(60), GUILayout.Height(34)))
                {
                    LoadItems(currentPage - 1);
                }

                GUI.enabled = true;

                EditorGUILayout.LabelField($"Page {currentPage}", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, GUILayout.ExpandWidth(true), GUILayout.Height(34));

                if (totalItems < currentPage * pageSize)
                    GUI.enabled = false;

                if (GUILayout.Button("Next", GUILayout.Width(60), GUILayout.Height(34)))
                {
                    LoadItems(currentPage + 1);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }


        private void RenderAudioClip(Asset asset, TempAudioData clipData)
        {
            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 10)
            });
            EditorGUILayout.BeginHorizontal();

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


            if (GUILayout.Button("Select", GUILayout.Width(60), GUILayout.Height(34)))
            {
                targetComponent.selectedAsset = asset;
                _editorComponent.selectedClipData = new RuntimeAudioEditor.TempAudioData();

                EditorUtility.SetDirty(targetComponent);
                EditorUtility.SetDirty(_editorComponent);

                Close();

                // Close the window after selection
                // Close();
            }
            //GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
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

        private Action PlayClipOnMainThread(AudioClip clip, TempAudioData asset)
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


        private void PlayClip(AudioClip clip, TempAudioData asset)
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

        private float GetPlaybackPosition(AudioSource audioSource)
        {
            if (audioSource == null || audioSource.clip == null) return 0f;
            return audioSource.time / audioSource.clip.length; // Normalize between 0 and 1
        }
    }
}
