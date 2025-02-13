using UnityEngine;

public static class AudioCacheReader
{
    public static AudioClip Load(string id)
    {
        return Resources.Load<AudioClip>($"audio/{id}");
    }
}
