
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuntimeSounds.Api
{
    public class UserApi : BaseApi
    {
        public const string Resource = "users";

        private readonly RuntimeSoundsSettings _settings;

        public UserApi(RuntimeSoundsSettings settings) : base(settings)
        {
            _settings = settings;
        }

        public virtual async Task<UserGetResponse> VerifyApiKeyAsync(string apiKey, RequestCallbacks callbacks = null)
        {
            return await GetAsync<UserGetResponse>(new Request()
            {
                Url = $"{_settings.ApiBaseUrl}/v1/{Resource}/api-key-user",
                Callbacks = callbacks,
                Headers = new Dictionary<string, string>()
                {
                    { "x-api-key", apiKey }
                }
            });
        }
    }
}