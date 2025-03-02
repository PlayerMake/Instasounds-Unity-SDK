using RuntimeSounds.Api;
using RuntimeSounds.Editor.UI.Components;
using RuntimeSounds.V1;
using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Windows
{
    public class GenerateAudioWindow : EditorWindow
    {
        RuntimeSoundsSettings settings;
        private LoadingIndicator loadingIndicator;

        private static bool generationLoading = false;

        private static string prompt = "";
        private static string assetName = "";
        private static int duration = 1;

        private Asset generatedAsset;
        private EditorAudioClip clipData = new EditorAudioClip();
        private GameObject previewGameObject;

        private EditorApplication.CallbackFunction updateCallback;
        private EditorApplication.CallbackFunction repaintCallback;
        private double nextRepaintTime = 0;

        [MenuItem("Tools/Runtime Sounds/Generate Audio", false, 0)]
        public static void Generate()
        {
            var window = GetWindow<GenerateAudioWindow>("Audio Generator");
            window.minSize = new Vector2(400, 240);
        }

        private void OnEnable()
        {
            loadingIndicator = new LoadingIndicator();

            if (previewGameObject == null)
                previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("PreviewAudio", HideFlags.HideAndDontSave);

            settings = SettingsHelper.GetOrCreateAndGetSettings();

            UserVerification.GetQuota(settings.ApiKey);
            UserVerification.VerifyApiKey(settings, settings.ApiKey);
        }
        public static void Open()
        {
            var window = GetWindow<GenerateAudioWindow>("Audio Generator");
            window.minSize = new Vector2(380, 300);
        }

        private void OnGUI()
        {
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

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(6, 6, 0, 0)
            });


            if (!UserVerification.apiKeyValid)
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
            else
            {
                GUILayout.Label($"Runtime Sounds Audio Generation", new GUIStyle()
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    },
                    margin = new RectOffset(5, 0, 0, 0),
                    alignment = TextAnchor.MiddleCenter,
                });
            }

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 10, 0)
            });

            EditorGUILayout.EndVertical();

            if (UserVerification.apiKeyValid)
            {
                EditorGUILayout.LabelField("Input", new GUIStyle()
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    },
                    margin = new RectOffset(5, 0, 6, 0),
                });

                assetName = EditorGUILayout.TextField("Name*", assetName);

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                prompt = EditorGUILayout.TextField("Sound Description*", prompt, new GUIStyle(EditorStyles.textField)
                {
                    wordWrap = true,
                }, GUILayout.Height(60));

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                duration = EditorGUILayout.IntSlider("Ideal Duration (seconds)", duration, 1, 30);

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 10, 0)
                });

                EditorGUILayout.EndVertical();

                var exceededQuota = UserVerification.quota?.GenerationQuota == null
                    || UserVerification.quota?.GenerationQuota - UserVerification.quota?.GenerationCount <= 0;

                if ((!exceededQuota && (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(assetName))) || generationLoading)
                    GUI.enabled = false;

                if (GUILayout.Button(!exceededQuota ? (string.IsNullOrEmpty(generatedAsset?.Id) ? "Generate New Sound" : "Generate Another Sound") : "Quota Exceeded. Click to manage limits.", GUILayout.ExpandWidth(true), GUILayout.Height(34)))
                {
                    if (exceededQuota)
                    {
                        //TODO: use proper account flow link
                        Application.OpenURL("https://runtimesounds.com/account");
                    }
                    else
                    {
                        generationLoading = true;

                        try
                        {
                            RuntimeSoundsSdk
                                .GenerateAssetAsync(prompt, assetName, duration, new RequestCallbacks()
                                {
                                    OnError = (error) =>
                                    {
                                        UserVerification.GetQuota(settings.ApiKey);
                                    }
                                })
                                .ContinueWith(generationResponse =>
                                {
                                    generationLoading = false;
                                    generatedAsset = generationResponse.Result;
                                });
                        } catch
                        {
                            generationLoading = false;
                        }
                        //TODO: generate sound
                    }
                }

                GUI.enabled = true;

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 10, 0)
                });

                EditorGUILayout.EndVertical();

                if (generationLoading)
                {
                    loadingIndicator.Render("generating", new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                    }, GUILayout.ExpandWidth(true));
                }

                if (!string.IsNullOrEmpty(generatedAsset?.Id))
                {
                    EditorGUILayout.LabelField("Generated Sound", new GUIStyle()
                    {
                        fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState()
                        {
                            textColor = Color.white
                        },
                        margin = new RectOffset(5, 0, 4, 0),
                    });

                    if (generatedAsset != null && !string.IsNullOrEmpty(generatedAsset.Url))
                    {
                        RenderAudioClip(generatedAsset, clipData);
                    }

                    EditorGUILayout.LabelField("Your new sound was successfully generated and is now available for you to select and use in any Runtime Audio component.", new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                    });

                    EditorGUILayout.BeginVertical(new GUIStyle()
                    {
                        margin = new RectOffset(0, 0, 2, 0)
                    });

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.LabelField("You can find all of your generated sounds in Tools -> Runtime Sounds -> Browse Audio by setting the source to 'My sounds'.", new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                    });

                    EditorGUILayout.BeginVertical(new GUIStyle()
                    {
                        margin = new RectOffset(0, 0, 10, 0)
                    });

                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("View All Generated Sounds", GUILayout.ExpandWidth(true), GUILayout.Height(34)))
                    {
                        AudioSearchWindow.Open("my-sounds");
                    }

                }
            }

            EditorGUILayout.EndVertical();
        }

        private void RenderAudioClip(Asset asset, EditorAudioClip clipData)
        {
            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 10)
            });

            EditorAudio.DrawAudioClip(asset, clipData, previewGameObject, updateCallback);

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 8)
            });

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            EditorAudio.DrawPlayButton(asset, clipData, previewGameObject, updateCallback, true, true);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void OnDisable()
        {
            if (previewGameObject) DestroyImmediate(previewGameObject.gameObject);
        }
    }
}
