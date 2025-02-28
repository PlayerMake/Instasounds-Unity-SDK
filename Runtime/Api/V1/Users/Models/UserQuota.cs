using Newtonsoft.Json;
using System;

[Serializable]
public class UserQuota
{
    [JsonProperty("generationQuota")]
    public int GenerationQuota;

    [JsonProperty("downloadQuota")]
    public int DownloadQuota;

    [JsonProperty("currentGenerationCount")]
    public int GenerationCount;

    [JsonProperty("currentDownloadCount")]
    public int RequestCount;
}
