using UnityEngine;

namespace RuntimeSounds.Api
{
    public class RuntimeSoundsSettings : ScriptableObject
    {
        [SerializeField]
        private string _apiBaseUrl = "https://api.runtimesounds.com";

        public string ApiKey = "";

        public string ApiBaseUrl => _apiBaseUrl;
    }
}
