using RuntimeSounds.Api;
using RuntimeSounds.Editor.UI.Components;
using RuntimeSounds.Editor.Utils;
using RuntimeSounds.V1;
using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Windows
{
    public class DeveloperDetailsWindow : EditorWindow
    {
        RuntimeSoundsSettings settings;
        private Debouncer debouncer;
        private LoadingIndicator loadingIndicator;

        private bool apiKeyValid = false;
        private bool apiKeyValidationLoading = false;

        [MenuItem("Tools/Runtime Sounds", false, 0)]
        public static void Generate()
        {
            var window = GetWindow<DeveloperDetailsWindow>("Runtime Sounds");
            window.minSize = new Vector2(700, 240);
        }

        private void OnEnable()
        {
            debouncer = new Debouncer(0.5f);
            loadingIndicator = new LoadingIndicator();
            settings = Resources.Load<RuntimeSoundsSettings>("RuntimeSoundsSettings");

            if (settings == null)
            {
                settings = CreateInstance<RuntimeSoundsSettings>();

                EnsureResourcePathExists();

                AssetDatabase.CreateAsset(settings, "Assets/RuntimeSounds/Resources/RuntimeSoundsSettings.asset");
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            VerifyApiKey(settings.ApiKey);
        }
        public static void Open()
        {
            var window = GetWindow<DeveloperDetailsWindow>("Runtime Sounds");
            window.minSize = new Vector2(380, 300);
        }

        private void EnsureResourcePathExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/RuntimeSounds"))
                AssetDatabase.CreateFolder("Assets", "RuntimeSounds");

            if (!AssetDatabase.IsValidFolder("Assets/RuntimeSounds/Resources"))
                AssetDatabase.CreateFolder("Assets/RuntimeSounds", "Resources");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                if (string.IsNullOrEmpty(settings.ApiKey))
                {
                    GUILayout.Label($"Welcome to Runtime Sounds!", new GUIStyle()
                    {
                        fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState()
                        {
                            textColor = Color.white
                        },
                        margin = new RectOffset(5, 0, 0, 0),
                        alignment = TextAnchor.MiddleCenter,
                    });
                } else
                {
                    GUILayout.Label($"Runtime Sounds Settings", new GUIStyle()
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
            }

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(10, 10, 6, 0)
            });

            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.LabelField($"Please use the button below to login to Runtime Sounds, generate an API Key, and add it below.", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                }, GUILayout.ExpandWidth(true));

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Get API Key", GUILayout.ExpandWidth(true)))
                {
                    Application.OpenURL("https://runtimesounds.com/api-keys");
                }

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();
            }

            GUILayout.Label("Project Settings", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                },
                margin = new RectOffset(5, 0, 6, 0),
            });

            var newApiKey = EditorGUILayout.TextField("API Key", settings.ApiKey);

            if (!apiKeyValid && !apiKeyValidationLoading && !string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.LabelField("API Key is invalid.", new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.red
                    }
                });
            }

            if (apiKeyValid && !apiKeyValidationLoading && !string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.LabelField("API Key is valid.", new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.green
                    }
                });
            }

            if (apiKeyValidationLoading)
            {
                loadingIndicator.Render(new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                }, GUILayout.ExpandWidth(true));
            }

            if (newApiKey != settings.ApiKey)
            {
                debouncer.Execute(() => VerifyApiKey(newApiKey));
            }

            EditorGUILayout.EndVertical();
        }

        private void VerifyApiKey(string apiKey)
        {
            apiKeyValidationLoading = true;
            settings.ApiKey = apiKey;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (string.IsNullOrEmpty(apiKey))
            {
                apiKeyValidationLoading = false;
                return;
            }

            try
            {
                RuntimeSoundsSdk
                    .VerifyApiKeyAsync(apiKey)
                    .ContinueWith(p =>
                    {
                        apiKeyValidationLoading = false;

                        if (p.Result == null)
                        {
                            apiKeyValid = false;
                        }
                        else
                        {
                            apiKeyValid = true;
                        }

                    });
            } catch
            {
                apiKeyValidationLoading = false;
                apiKeyValid = false;
                Repaint();
            }
        }
    }
}
