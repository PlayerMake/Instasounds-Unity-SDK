using UnityEngine;

namespace Instasounds.Api
{
    public class InstasoundsSettings : ScriptableObject
    {
        [SerializeField]
        private string _apiBaseUrl = "https://api.playermake.com";

        public string ProjectId = "";

        /// <warning>
        /// Setting this property locally will mean that your API key is exposed in your local game build which is a security flaw.
        /// If bad actors gain control of your API key then they can perform actions acting as your Player Make account via the API.
        /// 
        /// However, the alternative route to setting up the SDK is to use a proxy server (see the docs) and this is a more
        /// complicated setup involving your own backend server. If exposing your API key in your game build is an acceptable
        /// risk for you as a trade-off for the convenience of not having to have a backend server then you can set this property.
        /// </warning>
        public string ApiKey = "";

        public string ApiProxyUrl = "";

        public string ApiBaseUrl => string.IsNullOrEmpty(ApiProxyUrl) ? _apiBaseUrl : ApiProxyUrl;
    }
}
