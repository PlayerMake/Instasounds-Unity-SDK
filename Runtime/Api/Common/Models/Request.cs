using System;
using System.Collections.Generic;

namespace Instasounds.Api
{
    public class Request
    {
        public string Url { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public RequestCallbacks Callbacks = new RequestCallbacks();
    }

    public class RequestWithBody<T> : Request
    {
        public T Payload { get; set; }
    }

    public class RequestCallbacks
    {
        public Action<float, int> OnProgress { get; set; }

        public Action<string> OnError { get; set; }

        public Action OnComplete { get; set; }
    }
}