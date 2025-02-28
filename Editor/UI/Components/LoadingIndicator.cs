using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Components
{
    public class LoadingIndicator
    {
        private int currentFrame = 0;
        private int maxFrame = 3;

        private float updateInterval = 0.2f;
        private float lastUpdateTime = 0f;

        public LoadingIndicator()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public void OnDestroy()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup - lastUpdateTime < updateInterval)
                return;
            
            lastUpdateTime = Time.realtimeSinceStartup;
            currentFrame = (currentFrame + 1) % (maxFrame + 1);
        }

        public void Render(string text, GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            if (currentFrame == 0)
            {
                EditorGUILayout.LabelField(text, style, layoutOptions);
            }

            if (currentFrame == 1)
            {
                EditorGUILayout.LabelField($"{text}.", style, layoutOptions);
            }

            if (currentFrame == 2)
            {
                EditorGUILayout.LabelField($"{text}..", style, layoutOptions);
            }

            if (currentFrame == 3)
            {
                EditorGUILayout.LabelField($"{text}...", style, layoutOptions);
            }
        }

        public void Render(GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            Render("loading", style, layoutOptions);
        }
    }
}