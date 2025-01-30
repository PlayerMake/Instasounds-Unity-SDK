using Instasounds.Api;
using UnityEditor;
using UnityEngine;

namespace Instasounds.Editor.UI.Windows
{
    public class DeveloperDetailsWindow : EditorWindow
    {
        InstasoundsSettings settings;

        [MenuItem("Tools/Instasounds", false, 0)]
        public static void Generate()
        {
            var window = GetWindow<DeveloperDetailsWindow>("Instasounds");
            window.minSize = new Vector2(700, 240);
        }

        private void OnEnable()
        {
            settings = Resources.Load<InstasoundsSettings>("PlayerMakeSettings");

            if (settings == null)
            {
                settings = CreateInstance<InstasoundsSettings>();

                EnsureResourcePathExists();

                AssetDatabase.CreateAsset(settings, "Assets/Instasounds/Resources/InstasoundsSettings.asset");
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void EnsureResourcePathExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Instasounds"))
                AssetDatabase.CreateFolder("Assets", "Instasounds");

            if (!AssetDatabase.IsValidFolder("Assets/Instasounds/Resources"))
                AssetDatabase.CreateFolder("Assets/Instasounds", "Resources");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Welcome to the Instasounds SDK!", new GUIStyle()
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

            GUILayout.Space(10);

            GUILayout.Label("Project Settings", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() {
                    textColor = Color.white
                },
                margin = new RectOffset(5, 0, 0, 0),
            });
            settings.ProjectId = EditorGUILayout.TextField("Project ID", settings.ProjectId);

            GUILayout.Space(5);

            GUILayout.Label("Auth Settings", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                },
                margin = new RectOffset(5, 0, 0, 0),
            });

            settings.ApiProxyUrl = EditorGUILayout.TextField("API Proxy Url", settings.ApiProxyUrl);
            settings.ApiKey = EditorGUILayout.TextField("API Key", settings.ApiKey);

            using (new GUILayout.HorizontalScope(new GUIStyle()
            {
                margin = new RectOffset(5, 5, 5, 0)
            }))
            {
                EditorGUILayout.HelpBox(
                    "Setting the API key field will cause a security risk for your application, as it means that your API key can discovered in your build. We suggest setting the proxy url instead. See our docs for details.",
                    MessageType.Info
                );
            }
        }
    }
}
