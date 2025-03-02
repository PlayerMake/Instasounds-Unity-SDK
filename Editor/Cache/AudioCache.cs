using RuntimeSounds.V1;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class AudioCache
{
    private static RuntimeSounds.Editor.Cache.Cache cache;
    private static string pathToImport;

    public static bool IsImportInProgress => !String.IsNullOrEmpty(pathToImport);

    private static void Init()
    {
        if (cache == null)
            cache = new RuntimeSounds.Editor.Cache.Cache("audio");
    }

    public static async Task Save(byte[] bytes, string id)
    {
        if (IsImportInProgress)
            return;

        Init();

        var path = $"{cache.CacheDirectory}/{id}.wav";

        await File.WriteAllBytesAsync(path, bytes);

        pathToImport = path;
        EditorApplication.update += ImportAudio;
    }

    private static void ImportAudio()
    {
        EditorApplication.update -= ImportAudio;

        AssetDatabase.ImportAsset(pathToImport, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        pathToImport = string.Empty;
    }

    public static void Remove(string id)
    {
        Init();

        AssetDatabase.DeleteAsset($"{cache.CacheDirectory}/{id}.wav");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static AudioClip Load(string id)
    {
        Init();

        RuntimeSoundsSdk.NotifyOfLoad(id, true);

        return AssetDatabase.LoadAssetAtPath<AudioClip>($"{cache.CacheDirectory}/{id}.wav");
    }

    public static bool Exists(string id)
    {
        Init();

        return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{cache.CacheDirectory}/{id}.wav") != null;
    }
}
