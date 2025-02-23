using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace RuntimeSounds.Api
{
    public abstract class BaseApi
    {
        private readonly RuntimeSoundsSettings _settings;

        public BaseApi(RuntimeSoundsSettings settings)
        {
            _settings = settings;
        }

        protected virtual async Task<TResponse> PostAsync<TResponse, TPayload>(RequestWithBody<TPayload> request) where TResponse : Response, new()
        {
            var serializedPayload = JsonConvert.SerializeObject(request.Payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return await SendAsync<TResponse>(new BaseRequest
            {
                Headers = request.Headers,
                Method = "POST",
                SerializedPayload = serializedPayload,
                Url = request.Url
            }, request.Callbacks);
        }

        protected virtual async Task<TResponse> GetAsync<TResponse>(Request request) where TResponse : Response, new()
        {
            return await SendAsync<TResponse>(new BaseRequest
            {
                Headers = request.Headers,
                Method = "GET",
                Url = request.Url
            }, request.Callbacks);
        }

        private async Task<TResponse> SendAsync<TResponse>(BaseRequest request, RequestCallbacks callbacks
        ) where TResponse : Response, new()
        {
            using var webRequest = new UnityWebRequest();
            webRequest.url = request.Url;
            webRequest.method = request.Method;
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("X-API-KEY", _settings.ApiKey);

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(request.SerializedPayload))
            {
                var payloadBytes = Encoding.UTF8.GetBytes(request.SerializedPayload);
                webRequest.uploadHandler = new UploadHandlerRaw(payloadBytes);
            }

            var asyncOperation = webRequest.SendWebRequest();
            var progressPassCount = 0;

            while (!asyncOperation.isDone)
            {
                callbacks?.OnProgress?.Invoke(asyncOperation.progress, progressPassCount);
                progressPassCount++;
                await Task.Yield();
            }

            callbacks?.OnProgress?.Invoke(asyncOperation.progress, progressPassCount);

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callbacks?.OnComplete?.Invoke();

                return JsonConvert.DeserializeObject<TResponse>(webRequest.downloadHandler.text);
            }

            callbacks?.OnError?.Invoke(webRequest.error);

            return new TResponse()
            {
                IsSuccess = false,
                Status = webRequest.responseCode,
                Error = webRequest.error,
            };
        }

        private class BaseRequest
        {
            public string Url { get; set; }

            public string Method { get; set; }

            public string SerializedPayload { get; set; }

            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        }
    }
}
