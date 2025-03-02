using RuntimeSounds.V1;
using UnityEngine;

public static class AudioCacheReader
{
    public static AudioClip Load(string id)
    {
        RuntimeSoundsSdk.NotifyOfLoad(id, true);

        return Resources.Load<AudioClip>($"audio/{id}");
    }
}
