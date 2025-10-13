using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core
{
    public class RestApiClient
    {
        public string BaseUrl { get; private set; }

        private readonly CoroutineRunner _coroutineRunner;
        private readonly IJsonService _jsonService;
        private readonly ILogger _logger;

        private readonly List<Action<UnityWebRequest>>  _requestInterceptors = new List<Action<UnityWebRequest>>();
        private readonly List<Func<UnityWebRequest, IEnumerator>> _responseInterceptors = new List<Func<UnityWebRequest, IEnumerator>>();
        
        public RestApiClient(RestApiClientOptions options, CoroutineRunner coroutineRunner, IJsonService jsonService, ILogger logger)
        {
            BaseUrl = options.BaseUrl;
            _coroutineRunner = coroutineRunner;
            _jsonService = jsonService;
            _logger = logger;
        }

        private RequestOptions CreateDefaultRequestOptions()
        {
            return new RequestOptions()
            {
                DownloadHandler = new DownloadHandlerBuffer(),
            };
        }
        
        public void UseRequestInterceptor(Action<UnityWebRequest> interceptor)
        {
            _requestInterceptors.Add(interceptor);
        }
        
        public void UseResponseInterceptor(Func<UnityWebRequest, IEnumerator> interceptor)
        {
            _responseInterceptors.Add(interceptor);
        }
        
        public RestApiOperation Get(string url, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Method = UnityWebRequest.kHttpVerbGET;

            return SendRequest(url, options);
        }
        
        public RestApiOperation<T> Get<T>(string url, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Method = UnityWebRequest.kHttpVerbGET;
            
            return SendRequest<T>(url, options);
        }

        public RestApiOperation Post(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbPOST;

            return SendRequest(url, options);
        }
        
        public RestApiOperation<T> Post<T>(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbPOST;

            return SendRequest<T>(url, options);
        }

        public RestApiOperation Put(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbPUT;

            return SendRequest(url, options);
        }
        
        public RestApiOperation<T> Put<T>(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbPUT;

            return SendRequest<T>(url, options);
        }
        
        public RestApiOperation<T> Patch<T>(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbCREATE;

            return SendRequest<T>(url, options);
        }

        public RestApiOperation Patch(string url, object body, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Body = body;
            options.Method = UnityWebRequest.kHttpVerbCREATE;

            return SendRequest(url, options);
        }
        
        public RestApiOperation Delete(string url, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Method = UnityWebRequest.kHttpVerbDELETE;
            
            return SendRequest(url, options);
        }
        
        public RestApiOperation<T> Delete<T>(string url, RequestOptions options = null)
        {
            if (options == null)
            {
                options = CreateDefaultRequestOptions();
            }

            options.Method = UnityWebRequest.kHttpVerbDELETE;
            
            return SendRequest<T>(url, options);
        }
        
        private RestApiOperation SendRequest(string route, RequestOptions options)
        {
            RestApiOperation response = new RestApiOperation();

            string url = GetUrl(route);

            _coroutineRunner.StartCoroutine(SendRequest(url, response, options));
            
            return response;
        }
        
        private RestApiOperation<T> SendRequest<T>(string route, RequestOptions options)
        {
            RestApiOperation<T> response = new RestApiOperation<T>();

            string url = GetUrl(route);

            _coroutineRunner.StartCoroutine(SendRequest(url, response, options));
            
            return response;
        }
        
        private IEnumerator SendRequest(string url, BaseRestApiOperation response, RequestOptions options)
        {
            _logger.Log($"Send request: {url}");

            UnityWebRequest request = new UnityWebRequest(url, options.Method);

            if (options.Body != null)
            {
                string bodyJson = _jsonService.ToJson(options.Body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (options.DownloadHandler == null)
            {
                options.DownloadHandler = new DownloadHandlerBuffer();
            }

            request.downloadHandler = options.DownloadHandler;

            if (options.Headers != null)
            {
                foreach (var header in options.Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            foreach (var interceptor in _requestInterceptors)
            {
                interceptor.Invoke(request);
            }
                
            response.Initialize(request);
                
            yield return request.SendWebRequest();

            foreach (var interceptor in _responseInterceptors)
            {
                yield return interceptor.Invoke(request);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                _logger.Error($"{request.error} {request.downloadHandler.text}");
            }

            response.Complete();
        }

        public string GetUrl(string route)
        {
            return BaseUrl + route;
        }
    }
}