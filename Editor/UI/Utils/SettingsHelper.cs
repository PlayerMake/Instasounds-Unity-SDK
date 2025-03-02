using RuntimeSounds.Api;
using UnityEditor;
using UnityEngine;

public static class SettingsHelper
{
    public static RuntimeSoundsSettings GetOrCreateAndGetSettings()
    {
        var settings = Resources.Load<RuntimeSoundsSettings>("RuntimeSoundsSettings");

        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<RuntimeSoundsSettings>();

            EnsureResourcePathExists();

            AssetDatabase.CreateAsset(settings, "Assets/Runtime Sounds/Resources/RuntimeSoundsSettings.asset");
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return settings;
    }

    private static void EnsureResourcePathExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds"))
            AssetDatabase.CreateFolder("Assets", "Runtime Sounds");

        if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds/Resources"))
            AssetDatabase.CreateFolder("Assets/Runtime Sounds", "Resources");
    }
}
