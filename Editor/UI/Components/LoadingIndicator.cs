using UnityEditor;
using UnityEngine;

namespace RuntimeSounds.Editor.UI.Components
{
    public class LoadingIndicator
    {
        private int currentFrame = 0;
        private int maxFrame = 3;

        private float updateInterval = 0.3f;
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

        public void Render(params GUILayoutOption[] layoutOptions)
        {
            if (currentFrame == 0)
            {
                EditorGUILayout.LabelField("loading", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, layoutOptions);
            }

            if (currentFrame == 1)
            {
                EditorGUILayout.LabelField("loading.", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, layoutOptions);
            }

            if (currentFrame == 2)
            {
                EditorGUILayout.LabelField("loading..", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, layoutOptions);
            }

            if (currentFrame == 3)
            {
                EditorGUILayout.LabelField("loading...", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                }, layoutOptions);
            }
        }
    }
}