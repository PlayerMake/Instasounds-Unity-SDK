using System.IO;
using AssetDatabase = UnityEditor.AssetDatabase;
using TextAsset = UnityEngine.TextCore.Text.TextAsset;

namespace RuntimeSounds.Editor.Cache
{
    public class Cache
    {
        private readonly string _name;

        public string CacheDirectory => CacheConfig.BaseDirectory + _name;

        public Cache(string name)
        {
            _name = name;

            EnsureFoldersExist();
        }

        private void EnsureFoldersExist()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds"))
                AssetDatabase.CreateFolder("Assets", "Runtime Sounds");

            if (!AssetDatabase.IsValidFolder("Assets/Runtime Sounds/Resources"))
                AssetDatabase.CreateFolder("Assets/Runtime Sounds", "Resources");

            if (AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Runtime Sounds/Resources/README.txt") == null)
                CreateTextAsset("README.txt",
                    "This folder is managed by the Runtime Sounds SDK.",
                    "Assets/Runtime Sounds/Resources"
                );

            if (!AssetDatabase.IsValidFolder($"Assets/Runtime Sounds/Resources/{_name}"))
                AssetDatabase.CreateFolder("Assets/Runtime Sounds/Resources", _name);

            AssetDatabase.Refresh();
        }

        private static void CreateTextAsset(string name, string content, string directory)
        {
            File.WriteAllText($"{directory}/{name}", content);
        }
    }
}