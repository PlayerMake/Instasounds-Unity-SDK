using RuntimeSounds.Api;
using RuntimeSounds.Editor.UI.Components;
using RuntimeSounds.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Windows
{
    public class DeveloperDetailsWindow : EditorWindow
    {
        RuntimeSoundsSettings settings;

        private Debouncer debouncer;
        private LoadingIndicator loadingIndicator;

        [MenuItem("Tools/Runtime Sounds/Account", false, 1)]
        public static void Generate()
        {
            var window = GetWindow<DeveloperDetailsWindow>("Runtime Sounds");
            window.minSize = new Vector2(400, 240);
        }

        private void OnEnable()
        {
            debouncer = new Debouncer(0.5f);
            loadingIndicator = new LoadingIndicator();
            settings = SettingsHelper.GetOrCreateAndGetSettings();

            UserVerification.GetQuota(settings.ApiKey);
            UserVerification.VerifyApiKey(settings, settings.ApiKey);

        }
        public static void Open()
        {
            var window = GetWindow<DeveloperDetailsWindow>("Runtime Sounds");
            window.minSize = new Vector2(380, 300);
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
                    Application.OpenURL("https://runtimesounds.com/generate-api-key");
                }

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(0, 0, 6, 0)
                });

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.LabelField("Project Settings", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                },
                margin = new RectOffset(5, 0, 6, 0),
            });

            var newApiKey = EditorGUILayout.TextField("API Key", settings.ApiKey);

            if (!UserVerification.apiKeyValid && !UserVerification.apiKeyValidationLoading && !string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.LabelField("API Key is invalid.", new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.red
                    }
                });
            }

            if (UserVerification.apiKeyValid && !UserVerification.apiKeyValidationLoading && !string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.LabelField("API Key is valid.", new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.green
                    }
                });
            }

            if (UserVerification.apiKeyValidationLoading)
            {
                loadingIndicator.Render(new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                }, GUILayout.ExpandWidth(true));
            }

            if (newApiKey != settings.ApiKey)
            {
                debouncer.Execute(() => {
                    UserVerification.VerifyApiKey(settings, newApiKey);
                    UserVerification.GetQuota(newApiKey);
                });
            }

            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                margin = new RectOffset(10, 10, 10, 0)
            });

            EditorGUILayout.EndVertical();


            EditorGUILayout.LabelField("Account Stats", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                },
                margin = new RectOffset(5, 0, 10, 0),
            });

            var requestPercentageUsage = UserVerification.quota?.DownloadQuota == null || UserVerification.quota?.DownloadQuota == 0 ? 1 : (UserVerification.quota.RequestCount / UserVerification.quota.DownloadQuota);
            var generationPercentageUsage = UserVerification.quota?.GenerationQuota == null || UserVerification.quota?.GenerationQuota == 0 ? 1 : (UserVerification.quota.GenerationCount / UserVerification.quota.GenerationQuota);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Audio clip downloads in the last 30 days: ", GUILayout.Width(250));

            if (!UserVerification.quotaLoading)
            {
                EditorGUILayout.LabelField((UserVerification.quota?.RequestCount ?? 0) + "/" + (UserVerification.quota?.DownloadQuota ?? 0), new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = requestPercentageUsage >= 1 ? Color.red : (requestPercentageUsage >= 0.8 ? Color.yellow : Color.green)
                    }
                });
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio clips generated in the last 365 days:", GUILayout.Width(250));

            if (!UserVerification.quotaLoading)
            {
                EditorGUILayout.LabelField((UserVerification.quota?.GenerationCount ?? 0) + "/" + (UserVerification.quota?.GenerationQuota ?? 0), new GUIStyle(EditorStyles.label)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = generationPercentageUsage >= 1 ? Color.red : (generationPercentageUsage >= 0.8 ? Color.yellow : Color.green)
                    }
                });
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
