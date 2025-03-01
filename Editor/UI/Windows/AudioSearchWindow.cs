using RuntimeSounds.Api;
using RuntimeSounds.Editor.UI.Components;
using RuntimeSounds.Editor.Utils;
using RuntimeSounds.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Windows
{
    public class AudioSearchWindow : EditorWindow
    {
        RuntimeSoundsSettings settings;

        private static readonly int authedPageSize = 7;
        private static readonly int unauthedPageSize = 5;
        private static int pageSize;

        private static readonly Option[] sortOptions =
            new List<Option>() {
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
                } }
        .ToArray();

        private static readonly Option[] tierOptions = 
            new List<Option>() {
                new Option()
                {
                    Label = "All",
                    Value = null
                },
                new Option()
                {
                    Label = "Free",
                    Value = "free"
                },
                new Option()
                {
                    Label = "Basic",
                    Value = "basic"
                } }
        .ToArray();

        private static readonly Option[] sourceOptions =
            new List<Option>() {
                new Option()
                {
                    Label = "Runtime Sounds",
                    Value = null
                },
                new Option()
                {
                    Label = "Freesound",
                    Value = "freesound"
                },
                new Option()
                {
                    Label = "My sounds",
                    Value = "my-sounds"
                }
            }
        .ToArray();

        private static RuntimeAudio _component;
        private static RuntimeAudioEditor _editorComponent;

        private static string searchQuery = "";
        private static string selectedSort = "relevance";
        private static string selectedTier = null;
        private static string selectedSource = null;

        private Debouncer debouncer;

        public class DownloadedData
        {
            public EditorAudioClip SceneData = new EditorAudioClip();

            public Asset Asset;
        }

        private GameObject previewGameObject;

        private SelectInput sortSelectInput;
        private SelectInput tierSelectInput;
        private static SelectInput sourceSelectInput;

        private LoadingIndicator loadingIndicator;

        private EditorApplication.CallbackFunction updateCallback;
        private EditorApplication.CallbackFunction repaintCallback;

        private double nextRepaintTime = 0;

        private static int totalItems = 0;
        private static int currentPage = 1;

        private static bool loading = false;
        private static bool forceClose = false;

        private static string tier = string.Empty;
        private static bool tierLoading = false;

        private static List<DownloadedData> foundClips = new List<DownloadedData>();

        private string apiKey;

        [MenuItem("Tools/Runtime Sounds/Browse Audio", false, 0)]
        public static void Generate()
        {
            _component = null;
            _editorComponent = null;
            var window = GetWindow<AudioSearchWindow>("Browse Audio");
            window.minSize = new Vector2(490, 500);
        }

        void OnEnable()
        {
            settings = Resources.Load<RuntimeSoundsSettings>("RuntimeSoundsSettings");

            if (settings == null)
            {
                settings = CreateInstance<RuntimeSoundsSettings>();

                EnsureResourcePathExists();

                AssetDatabase.CreateAsset(settings, "Assets/Runtime Sounds/Resources/RuntimeSoundsSettings.asset");
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            debouncer = new Debouncer(0.5f);

            if (!previewGameObject)
            {
                previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("PreviewAudio", HideFlags.HideAndDontSave);
            }

            if (sortSelectInput == null)
            {
                sortSelectInput = new SelectInput();
                sortSelectInput.Init(sortOptions, selectedSort);
            }

            if (tierSelectInput == null)
            {
                tierSelectInput = new SelectInput();
                tierSelectInput.Init(tierOptions, selectedTier);
            }

            if (sourceSelectInput == null)
            {
                sourceSelectInput = new SelectInput();
                sourceSelectInput.Init(sourceOptions, selectedSource);
            }

            if (loadingIndicator == null)
            {
                loadingIndicator = new LoadingIndicator();
            }

            apiKey = settings.ApiKey;
            VerifyTier();
        }

        private void Update()
        {
            if (apiKey != settings.ApiKey)
            {
                apiKey = settings.ApiKey;
                VerifyTier();
            }
        }

        private void VerifyTier()
        {
            if (tierLoading)
                return;

            tierLoading = true;

            try
            {
                var threadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                RuntimeSoundsSdk
                    .VerifyApiKeyAsync(settings.ApiKey)
                    .ContinueWith(p =>
                    {
                        tier = p?.Result?.Tier;
                        tierLoading = false;

                        if (string.IsNullOrEmpty(tier))
                        {
                            pageSize = unauthedPageSize;
                        }
                        else
                        {
                            pageSize = authedPageSize;
                        }

                        LoadItems(currentPage);
                    }, threadScheduler);
            }
            catch
            {
                tierLoading = false;
            }
        }

        private void EnsureResourcePathExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds"))
                AssetDatabase.CreateFolder("Assets", "Runtime Sounds");

            if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds/Resources"))
                AssetDatabase.CreateFolder("Assets/Runtime Sounds", "Resources");
        }

        private static void LoadItems(int page)
        {
            loading = true;

            try
            {
                RuntimeSoundsSdk.ListAssetsAsync(
                    searchQuery,
                    selectedSort,
                    selectedTier,
                    selectedSource,
                    pageSize, (page - 1) * pageSize)
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

        public static void Open(string defaultSource) 
        {
            var window = GetWindow<AudioSearchWindow>("Audio Search");
            window.minSize = new Vector2(490, 500);

            selectedSource = defaultSource;

            sourceSelectInput = new SelectInput();
            sourceSelectInput.Init(sourceOptions, selectedSource);

            LoadItems(0);
        }

        public static void Open(RuntimeAudioEditor editorComponent, RuntimeAudio component)
        {
            var window = GetWindow<AudioSearchWindow>("Audio Picker");
            window.minSize = new Vector2(490, 500);

            _component = component;
            _editorComponent = editorComponent;
        }

        public static void ForceClose()
        {
            forceClose = true;
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

            if (!tierLoading & string.IsNullOrEmpty(tier))
            {
                EditorGUILayout.LabelField($"To start using Runtime Sounds, you first need to setup your account.", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                }, GUILayout.ExpandWidth(true));

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Setup Account", GUILayout.ExpandWidth(true)))
                {
                    DeveloperDetailsWindow.Open();
                }

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField($"You can still browse the sound library below, but you need to set your API Key in Tools -> Runtime Sounds -> Account before you can play, create, or use them.", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                }, GUILayout.ExpandWidth(true));

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 20, 0)
                });

                EditorGUILayout.EndVertical();
            }

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
                margin = new RectOffset(0, 0, 6, 0)
            });

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Source", GUILayout.Width(50));

            sourceSelectInput.Render((source) =>
            {
                selectedSource = source;
                LoadItems(0);

            }, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 6, 0)
            });

            EditorGUILayout.EndVertical();

            /* EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Tier", GUILayout.Width(50));

            tierSelectInput.Render((tier) =>
            {
                selectedTier = tier;
                LoadItems(0);

            }, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            EditorGUILayout.EndVertical(); */

            EditorGUILayout.BeginHorizontal();

            if (!tierLoading & !string.IsNullOrEmpty(tier))
            {
                EditorGUILayout.LabelField("Account Level: " + tier, GUILayout.Width(180));

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();
            }

            if (loading)
                loadingIndicator.Render(new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                }, GUILayout.ExpandWidth(true));

            if (!loading)
            {
                EditorGUILayout.LabelField($"{totalItems} sound{(totalItems == 1 ? "" : "s")} found", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleRight,
                }, GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            EditorGUILayout.EndVertical();

            if (selectedSource == "my-sounds" && !loading && totalItems == 0)
            {
                EditorGUILayout.LabelField("You haven't generated any sounds yet!", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                });

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Generate a Sound", GUILayout.Height(34)))
                {
                    GenerateAudioWindow.Open();
                }
            }

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


        private void RenderAudioClip(Asset asset, EditorAudioClip clipData)
        {
            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 10)
            });
            EditorGUILayout.BeginHorizontal();

            EditorAudio.DrawAudioClip(asset, clipData, previewGameObject, updateCallback, true);
            EditorAudio.DrawPlayButton(asset, clipData, previewGameObject, updateCallback, true);

            if (!string.IsNullOrEmpty(tier) && !(asset.IsPremium && tier == "Free Tier") && _component?.selectedAsset != null)
            {
                if (GUILayout.Button("Select", GUILayout.Width(60), GUILayout.Height(34)))
                {
                    _component.selectedAsset = asset;
                    _editorComponent.selectedClipData = new EditorAudioClip();

                    EditorUtility.SetDirty(_component);
                    EditorUtility.SetDirty(_editorComponent);

                    Close();
                }
            }

            if (string.IsNullOrEmpty(asset.Id) && !string.IsNullOrEmpty(asset.ExternalId) && !(asset.IsPremium && tier == "Free Tier"))
            {
                if (clipData.importing)
                    GUI.enabled = false;

                if (String.IsNullOrEmpty(tier))
                    GUI.enabled = false;

                if (GUILayout.Button(clipData.importing ? "Importing" : "Import", GUILayout.Width(138), GUILayout.Height(34)))
                {
                    clipData.importing = true;

                    RuntimeSoundsSdk.ImportAssetAsync(selectedSource, asset.ExternalId).ContinueWith(p =>
                    {
                        try
                        {
                            clipData.importing = false;

                            if (p.IsFaulted || p.Result == null)
                            {
                                Debug.LogError("Failed to import sound.");
                                return;
                            }

                            asset.Id = p.Result.Id;
                            asset.Url = p.Result.Url;
                            asset.PreviewUrl = p.Result.PreviewUrl;
                        }
                        catch (Exception)
                        {
                            Debug.LogError("Failed to import sound.");
                            return;
                        }
                    });
                }

                GUI.enabled = true;
            }

            if ((string.IsNullOrEmpty(tier) || (asset.IsPremium && tier == "Free Tier")) && !string.IsNullOrEmpty(asset.Id))
            {
                GUI.enabled = false;
                if (GUILayout.Button("Locked", GUILayout.Width(60), GUILayout.Height(34)))
                {
                }
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
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
    }
}
