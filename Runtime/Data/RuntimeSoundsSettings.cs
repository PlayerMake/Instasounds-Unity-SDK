using UnityEngine;

namespace RuntimeSounds.Api
{
    public class RuntimeSoundsSettings : ScriptableObject
    {
        [SerializeField]
        private string _apiBaseUrl = "http://localhost:3001";

        public string ProjectId = "";

        public string ApiKey = "";

        public string ApiBaseUrl => _apiBaseUrl;
    }
}
